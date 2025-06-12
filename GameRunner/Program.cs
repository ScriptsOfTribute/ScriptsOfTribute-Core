using System.CommandLine;
using System.Diagnostics;
using GameRunner;
using Bots;
using ScriptsOfTribute.AI;
using ScriptsOfTributeGRPC;
using System.CommandLine.Parsing;
using System.CommandLine.Invocation;
using System.Runtime.Loader;

var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
var botsDirectory = Path.Combine(currentDirectory.FullName, "Bots");

var aiType = typeof(AI);
var externalBotType = typeof(ExternalAIAdapter);
var botDlls = Directory.Exists(botsDirectory)
    ? Directory.GetFiles(botsDirectory, "*.dll")
    : Array.Empty<string>();

List<Type> allBots = botDlls
    .Select(f => AssemblyLoadContext.Default.LoadFromAssemblyPath(f))
    .SelectMany(a => a.GetTypes())
    .Where(t => aiType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
    .ToList();

BotInfo? cachedBot = null;
var returnValue = 0;
#region Options and arguments

var noOfRunsOption = CreateOption<int>("--runs", "Number of games to run.", 1, "-n");
var threadsOption = CreateOption<int>("--threads", "Number of CPU threads to use.", 1, "-t");
var logsOption = CreateOption<LogsEnabled>("--enable-logs", "Enable logging.", LogsEnabled.NONE, "-l");
var seedOption = CreateOption<ulong?>("--seed", "Specify RNG seed.", null, "-s");
var logFileDestination = CreateLogFileOption("--log-destination", "Directory for log files.", "-d");
var timeoutOption = CreateOption<int>("--timeout", "Game timeout in seconds.", 30, "-to");
var clientPortOption = CreateOption<int>("--client-port", "Base client port for gRPC bots.", 50000, "-cp");
var serverPortOption = CreateOption<int>("--server-port", "Base server port for gRPC bots.", 49000, "-sp");

var bot1NameArgument = CreateBotArgument("bot1", "Name of the first bot or command.");
var bot2NameArgument = CreateBotArgument("bot2", "Name of the second bot or command.");

var mainCommand = new RootCommand("A game runner for bots.")
{
    noOfRunsOption,
    threadsOption,
    logsOption,
    logFileDestination,
    seedOption,
    timeoutOption,
    clientPortOption,
    serverPortOption,
    bot1NameArgument,
    bot2NameArgument,
};

#endregion

#region Bot logic
BotInfo? FindBot(string name, out string? errorMessage)
{
    errorMessage = null;

    if (name.StartsWith("cmd:"))
    {
        var parts = name[4..].Split(' ', 2);
        return new ExternalBotInfo
        {
            BotType = typeof(ExternalAIAdapter),
            ProgramName = parts[0],
            FileName = parts.Length > 1 ? parts[1] : null
        };
    }
    if (name.StartsWith("grpc:"))
    {
        return new gRPCBotInfo
        {
            BotName = name[5..],
            BotType = typeof(gRPCBot),
            HostName = "localhost",
            ClientPort = 50000,
            ServerPort = 49000
        };
    }

    return FindInternalBot(name, out errorMessage) ?? throw new Exception(errorMessage);
}

BotInfo? FindInternalBot(string name, out string? errorMessage)
{
    errorMessage = null;
    var botInfo = new LocalBotInfo()
    {
        BotName = name,
    };
    bool findByFullName = name.Contains('.');
    if (cachedBot is not null && (findByFullName ? cachedBot.BotFullName : cachedBot.BotName) == name)
    {
        return cachedBot;
    }

    var botCount = allBots.Count(t => (findByFullName ? t.FullName : t.Name) == name);

    if (botCount == 0)
    {
        errorMessage = $"Bot {name} not found in any DLLs. List of bots found:\n";
        errorMessage += string.Join('\n', allBots.Select(b => b.FullName));
        return null;
    }

    if (botCount > 1 && !findByFullName)
    {
        errorMessage = "More than one bots with the same name found. Please, specify full name of the target bot: <namespace>.Name. Bots found:\n";
        errorMessage += string.Join('\n', allBots.Select(b => b.FullName));
        return null;
    }

    else if (botCount > 1 && findByFullName)
    {
        errorMessage = "More than one bots with the same full name found. This means you have different DLLs with the same namespaces and bot names.\n" +
                       "This use case is not yet supported. List of all found bots:\n";
        errorMessage += string.Join('\n', allBots.Select(b => b.FullName));
        return null;
    }

    botInfo.BotType = allBots.First(t => (findByFullName ? t.FullName : t.Name) == name);
    cachedBot = botInfo;

    if (cachedBot.BotType.GetConstructor(Type.EmptyTypes) is null)
    {
        errorMessage = $"Bot {name} bot can't be instantiated as it doesn't provide a parameterless constructor.";
    }

    return cachedBot;
}

BotInfo? ParseBotArg(ArgumentResult arg)
{
    if (arg.Tokens.Count != 1)
    {
        arg.ErrorMessage = "Bot name must be a single token.";
        return null;
    }

    var bot = FindBot(arg.Tokens[0].Value, out var errorMessage);
    if (errorMessage is not null)
    {
        arg.ErrorMessage = errorMessage;
        return null;
    }

    return bot!;
}
#endregion

#region Prepare game

ScriptsOfTribute.AI.ScriptsOfTribute PrepareGame(AI bot1, AI bot2, LogsEnabled enableLogs, ulong seed, LogFileNameProvider? logProvider, int timeout)
{
    var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1, bot2, TimeSpan.FromSeconds(timeout))
    {
        Seed = seed,
    };

    switch (enableLogs)
    {
        case LogsEnabled.P1:
            game.P1LoggerEnabled = true;
            break;
        case LogsEnabled.P2:
            game.P2LoggerEnabled = true;
            break;
        case LogsEnabled.NONE:
            break;
        case LogsEnabled.BOTH:
            game.P1LoggerEnabled = true;
            game.P2LoggerEnabled = true;
            break;
        default:
            throw new ArgumentOutOfRangeException(nameof(enableLogs), enableLogs, null);
    }

    if (logProvider is not null)
    {
        var (p1LogDest, p2LogDest) = logProvider.GetForPlayers(seed, game.P1LoggerEnabled, game.P2LoggerEnabled);
        game.P1LogTarget = p1LogDest;
        game.P2LogTarget = p2LogDest;
    }

    return game;
}

#endregion

#region Main command handler

mainCommand.SetHandler((InvocationContext context) =>
{
    int runs = context.ParseResult.GetValueForOption(noOfRunsOption);
    int threads = context.ParseResult.GetValueForOption(threadsOption);
    LogsEnabled logs = context.ParseResult.GetValueForOption(logsOption);
    LogFileNameProvider? logProvider = context.ParseResult.GetValueForOption(logFileDestination);
    ulong? seed = context.ParseResult.GetValueForOption(seedOption);
    int timeout = context.ParseResult.GetValueForOption(timeoutOption);
    int baseClientPort = context.ParseResult.GetValueForOption(clientPortOption);
    int baseServerPort = context.ParseResult.GetValueForOption(serverPortOption);
    BotInfo? bot1Info = context.ParseResult.GetValueForArgument(bot1NameArgument);
    BotInfo? bot2Info = context.ParseResult.GetValueForArgument(bot2NameArgument);

    if (bot1Info is null || bot2Info is null)
    {
        Console.Error.WriteLine("ERROR: Bots were not parsed correctly.");
        returnValue = -1;
        return;
    }

    string baseHost = "localhost";

    if (!ValidateInputs(threads, timeout)) return;
    ulong actualSeed = seed ?? (ulong)new Random().NextInt64();

    if (threads == 1)
        RunSingleThreaded(runs, bot1Info, bot2Info, logs, logProvider, actualSeed, timeout, baseClientPort, baseServerPort, baseHost);
    else
        RunMultiThreaded(runs, threads, bot1Info, bot2Info, logs, logProvider, actualSeed, timeout, baseClientPort, baseServerPort, baseHost);
});

void RunSingleThreaded(
    int runs,
    BotInfo bot1Info,
    BotInfo bot2Info,
    LogsEnabled enableLogs,
    LogFileNameProvider? logFileNameProvider,
    ulong actualSeed,
    int timeout,
    int baseClientPort,
    int baseServerPort,
    string baseHost = "localhost"
)
{
    Console.WriteLine($"Running {runs} games - {bot1Info.BotName} vs {bot2Info.BotName}");
    var counter = new GameEndStatsCounter();
    var timeMeasurements = new long[runs];
    var granularWatch = new Stopwatch();
    var currentSeed = actualSeed;

    if (bot1Info is gRPCBotInfo grpcBotInfo1)
    {
        grpcBotInfo1.HostName = baseHost;
        grpcBotInfo1.ClientPort = baseClientPort;
        grpcBotInfo1.ServerPort = baseServerPort;
    }

    if (bot2Info is gRPCBotInfo grpcBotInfo2)
    {
        grpcBotInfo2.HostName = baseHost;
        grpcBotInfo2.ClientPort = baseClientPort+1;
        grpcBotInfo2.ServerPort = baseServerPort+1;
    }

    var bot1 = bot1Info.CreateBotInstance();
    var bot2 = bot2Info.CreateBotInstance();

    for (var i = 0; i < runs; i++)
    {
        var game = PrepareGame(bot1, bot2, enableLogs, currentSeed, logFileNameProvider, timeout);
        currentSeed += 1;

        granularWatch.Reset();
        granularWatch.Start();
        var (endReason, _) = game.Play();
        granularWatch.Stop();
        if (endReason.Reason == ScriptsOfTribute.Board.GameEndReason.BOT_EXCEPTION)
            Console.WriteLine(endReason);
        timeMeasurements[i] = granularWatch.ElapsedMilliseconds;
        counter.Add(endReason);
    }

    if (bot1 is gRPCBot grpcBot1)
    {
        grpcBot1.CloseConnection();
    }
    if (bot2 is gRPCBot grpcBot2)
    {
        grpcBot2.CloseConnection();
    }
    Console.WriteLine($"\nInitial seed used: {actualSeed}");
    Console.WriteLine($"Total time taken: {timeMeasurements.Sum()}ms");
    Console.WriteLine($"Average time per game: {timeMeasurements.Average()}ms");
    Console.WriteLine("\nStats from the games played:");
    Console.WriteLine(counter.ToString());
}

void RunMultiThreaded(
    int runs,
    int noOfThreads,
    BotInfo bot1Info,
    BotInfo bot2Info,
    LogsEnabled enableLogs,
    LogFileNameProvider? logFileNameProvider,
    ulong actualSeed,
    int timeout,
    int baseClientPort,
    int baseServerPort,
    string baseHost = "localhost"
)
{
    Console.WriteLine($"Running {runs} games with {noOfThreads} threads.");

    var gamesPerThread = runs / noOfThreads;
    var gamesPerThreadRemainder = runs % noOfThreads;
    var threads = new Task<List<ScriptsOfTribute.Board.EndGameState>>[noOfThreads];

    List<ScriptsOfTribute.Board.EndGameState> PlayGames(int amount, BotInfo bot1Info, BotInfo bot2Info, int threadNo, ulong seed)
    {
        var results = new ScriptsOfTribute.Board.EndGameState[amount];
        var timeMeasurements = new long[amount];
        var watch = new Stopwatch();

        var bot1 = bot1Info.CreateBotInstance();
        var bot2 = bot2Info.CreateBotInstance();

        for (var i = 0; i < amount; i++)
        {
            var game = PrepareGame(bot1, bot2, enableLogs, seed, logFileNameProvider, timeout);
            seed += 1;

            watch.Reset();
            watch.Start();
            var (endReason, _) = game.Play();
            watch.Stop();

            results[i] = endReason;
            timeMeasurements[i] = watch.ElapsedMilliseconds;
        }
        if (bot1 is gRPCBot grpcBot1)
        {
            grpcBot1.CloseConnection();
        }
        if (bot2 is gRPCBot grpcBot2)
        {
            grpcBot2.CloseConnection();
        }
        Console.WriteLine($"Thread #{threadNo} finished. Total: {timeMeasurements.Sum()}ms, average: {timeMeasurements.Average()}ms.");
        return results.ToList();
    }

    var watch = Stopwatch.StartNew();
    var currentSeed = actualSeed;

    for (var i = 0; i < noOfThreads; i++)
    {
        var additionalGames = gamesPerThreadRemainder-- > 0 ? 1 : 0;
        var gamesToPlay = gamesPerThread + additionalGames;
        var threadNo = i;
        var currentSeedCopy = currentSeed;
        var bot1ThreadInfo = bot1Info is gRPCBotInfo grpcBot1
            ? new gRPCBotInfo
            {
                BotName = grpcBot1.BotName,
                BotType = grpcBot1.BotType,
                HostName = grpcBot1.HostName,
                ClientPort = baseClientPort + i,
                ServerPort = baseServerPort + i,
            }
            : bot1Info;

        var bot2ThreadInfo = bot2Info is gRPCBotInfo grpcBot2
            ? new gRPCBotInfo
            {
                BotName = grpcBot2.BotName,
                BotType = grpcBot2.BotType,
                HostName = grpcBot2.HostName,
                ClientPort = baseClientPort + noOfThreads + i,
                ServerPort = baseServerPort + noOfThreads + i,
            }
            : bot2Info;
        threads[i] = Task.Factory.StartNew(() => PlayGames(gamesToPlay, bot1ThreadInfo, bot2ThreadInfo, threadNo, currentSeedCopy));
        currentSeed += (ulong)gamesToPlay;
    }

    Task.WaitAll(threads);

    var timeTaken = watch.ElapsedMilliseconds;

    var counter = new GameEndStatsCounter();
    threads.SelectMany(t => t.Result).ToList().ForEach(counter.Add);

    Console.WriteLine($"\nInitial seed used: {actualSeed}");
    Console.WriteLine($"Total time taken: {timeTaken}ms");
    Console.WriteLine("\nStats from the games played:");
    Console.WriteLine(counter.ToString());
}

#endregion

#region Helpers

Option<T> CreateOption<T>(string name, string description, T defaultValue, string alias = "")
{
    var option = new Option<T>(name, () =>  defaultValue, description);
    option.AddAlias(alias);
    return option;
}
    

Option<LogFileNameProvider?> CreateLogFileOption(string name, string description, string alias)
{
    var option = new Option<LogFileNameProvider?>(
         name: "--log-destination",
        description: "Log to files with names 'directory/<seed_bot.log>' instead of standard output. Specify the directory here.",
        isDefault: true,
        parseArgument: result =>
        {
            if (result.Tokens.Count == 0)
            {
                return null;
            }

            var dirName = result.Tokens.Single().Value;
            var dir = Directory.CreateDirectory(dirName);
            return new LogFileNameProvider(dir);
        }
    )
    {
        IsRequired = false,
    };
    option.AddAlias("-d");
    return option;
}

Argument<BotInfo?> CreateBotArgument(string name, string description) =>
    new(name, description: description, parse: ParseBotArg);

bool ValidateInputs(int threads, int timeout)
{
    if (threads < 1)
    {
        Console.Error.WriteLine("ERROR: Can't use less than 1 thread.");
        returnValue = -1;
        return false;
    }

    if (timeout < 0)
    {
        Console.Error.WriteLine("ERROR: Time limit can't be negative.");
        returnValue = -1;
        return false;
    }
    return true;
}

#endregion

mainCommand.Invoke(args);
return returnValue;
