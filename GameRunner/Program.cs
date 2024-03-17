
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using GameRunner;
using Bots;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

var aiType = typeof(AI);
var externalBotType = typeof(ExternalAIAdapter);
var allDlls = currentDirectory.GetFiles("*.dll").ToList();
List<Type> allBots = allDlls.Select(f => f.FullName)
    .Select(Assembly.LoadFile)
    .SelectMany(a => a.GetTypes())
    .Where(t => aiType.IsAssignableFrom(t) && !t.IsInterface)
    .ToList();

var noOfRunsOption = new Option<int>(
    name: "--runs",
    description: "Number of games to run.",
    getDefaultValue: () => 1);
noOfRunsOption.AddAlias("-n");

var threadsOption = new Option<int>(
    name: "--threads",
    description: "Number of CPU threads to use.",
    getDefaultValue: () => 1);
threadsOption.AddAlias("-t");

var logsOption = new Option<LogsEnabled>(
    name: "--enable-logs",
    description: "Enable logging (to standard output by default).",
    getDefaultValue: () => LogsEnabled.NONE);
logsOption.AddAlias("-l");

var seedOption = new Option<ulong?>(
    name: "--seed",
    description: "Specify seed for RNG. In case of multiple games, each subsequent game will be played with <seed+1>.",
    getDefaultValue: () => null);
seedOption.AddAlias("-s");

var logFileDestination = new Option<LogFileNameProvider?>(
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
    })
{
    IsRequired = false,
};
logFileDestination.AddAlias("-d");

var timeoutOption = new Option<int>(
    name: "--timeout",
    description: "Specify time limit in seconds that will be set for games. The default value is 30.",
    getDefaultValue: () => 30);
timeoutOption.AddAlias("-to");


BotInfo? cachedBot = null;

BotInfo? FindBot(string name, out string? errorMessage)
{
    errorMessage = null;
    var botInfo = new BotInfo();

    bool findByFullName = name.Contains('.');
    bool isExternalBot = name.StartsWith("cmd:");

    if (isExternalBot)
    {
        name = name.Substring(4);
        var splittedCommand = name.Split(' ', 2);
        Console.WriteLine(name);
        Console.WriteLine($"{splittedCommand[0]}, {splittedCommand[1]}");
        botInfo.BotType = externalBotType;
        botInfo.ProgramName = splittedCommand[0];
        botInfo.FileName = splittedCommand[1];
        return botInfo;
    }

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
    // TODO: Support also specifying which file to use.
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

var bot1NameArgument = new Argument<BotInfo?>(name: "bot1Name", description: "Name of the first bot.", parse: ParseBotArg);
var bot2NameArgument = new Argument<BotInfo?>(name: "bot2Name", description: "Name of the second bot.", parse: ParseBotArg);

var mainCommand = new RootCommand("A game runner for bots.")
{
    noOfRunsOption,
    threadsOption,
    logsOption,
    logFileDestination,
    seedOption,
    timeoutOption,
    bot1NameArgument,
    bot2NameArgument,
};

ScriptsOfTribute.AI.ScriptsOfTribute PrepareGame(AI bot1, AI bot2, LogsEnabled enableLogs, ulong seed, LogFileNameProvider? logFileNameProvider, int timeout)
{
    var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1!, bot2!, TimeSpan.FromSeconds(timeout));
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

    game.Seed = seed;

    if (logFileNameProvider is not null)
    {
        var (p1LogDest, p2LogDest) =
            logFileNameProvider.GetForPlayers(game.Seed, game.P1LoggerEnabled, game.P2LoggerEnabled);   
        game.P1LogTarget = p1LogDest;
        game.P2LogTarget = p2LogDest;
    }

    return game;
}

var returnValue = 0;
mainCommand.SetHandler((runs, noOfThreads, enableLogs, logFileNameProvider, maybeSeed, timeout, bot1Info, bot2Info) =>
{
    if (noOfThreads < 1)
    {
        Console.Error.WriteLine("ERROR: Can't use less than 1 thread.");
        returnValue = -1;
    }

    if (timeout < 0)
    {
        Console.Error.WriteLine("ERROR: Time limit can't be negative.");
        returnValue = -1;
    }

    ulong actualSeed;
    if (maybeSeed is null)
    {
        actualSeed = (ulong)new Random().NextInt64();
    }
    else
    {
        actualSeed = (ulong)maybeSeed;
    }

    if (noOfThreads == 1)
    {
        Console.WriteLine($"Running {runs} games - {bot1Info!.BotType.Name} vs {bot2Info!.BotType.Name}");

        var counter = new GameEndStatsCounter();

        var timeMeasurements = new long[runs];

        var granularWatch = new Stopwatch();
        var currentSeed = actualSeed;
        var bot1 = bot1Info.IsExternal ? (AI?)Activator.CreateInstance(bot1Info.BotType, bot1Info.ProgramName, bot1Info.FileName) : (AI?)Activator.CreateInstance(bot1Info.BotType);
        var bot2 = bot2Info.IsExternal ? (AI?)Activator.CreateInstance(bot2Info.BotType, bot2Info.ProgramName, bot2Info.FileName) : (AI?)Activator.CreateInstance(bot2Info.BotType);
        for (var i = 0; i < runs; i++)
        {
            var game = PrepareGame(bot1!, bot2!, enableLogs, currentSeed, logFileNameProvider, timeout);
            currentSeed += 1;

            granularWatch.Reset();
            granularWatch.Start();
            var (endReason, _) = game.Play();
            granularWatch.Stop();

            var gameTimeTaken = granularWatch.ElapsedMilliseconds;
            timeMeasurements[i] = gameTimeTaken;

            counter.Add(endReason);
        }

        Console.WriteLine($"\nInitial seed used: {actualSeed}");
        Console.WriteLine($"Total time taken: {timeMeasurements.Sum()}ms");
        Console.WriteLine($"Average time per game: {timeMeasurements.Average()}ms");
        Console.WriteLine("\nStats from the games played:");
        Console.WriteLine(counter.ToString());    
    }
    else
    {
        if (enableLogs != LogsEnabled.NONE && logFileNameProvider is null)
        {
            Console.Error.WriteLine("ERROR: Logs to stdout are not supported with multi-threading. Specify file destination.");
            returnValue = -1;
            return;
        }

        if (noOfThreads > Environment.ProcessorCount)
        {
            Console.Error.WriteLine($"WARNING: More threads ({noOfThreads}) specified than logical processor count ({Environment.ProcessorCount}).");
        }

        var gamesPerThread = runs / noOfThreads;
        var gamesPerThreadRemainder = runs % noOfThreads;
        var threads = new Task<List<EndGameState>>[noOfThreads];
        
        List<EndGameState> PlayGames(int amount, BotInfo bot1Info, BotInfo bot2Info, int threadNo, ulong seed)
        {
            var results = new EndGameState[amount];
            var timeMeasurements = new long[amount];
            var watch = new Stopwatch();
            var bot1 = bot1Info.IsExternal ? (AI?)Activator.CreateInstance(bot1Info.BotType, bot1Info.ProgramName, bot1Info.FileName) : (AI?)Activator.CreateInstance(bot1Info.BotType);
            var bot2 = bot2Info.IsExternal ? (AI?)Activator.CreateInstance(bot2Info.BotType, bot2Info.ProgramName, bot2Info.FileName) : (AI?)Activator.CreateInstance(bot2Info.BotType);
            for (var i = 0; i < amount; i++)
            {
                var game = PrepareGame(bot1!, bot2!, enableLogs, seed, logFileNameProvider, timeout);
                seed += 1;

                watch.Reset();
                watch.Start();
                var (endReason, _) = game.Play();
                watch.Stop();
                results[i] = endReason;
                timeMeasurements[i] = watch.ElapsedMilliseconds;
            }
        
            Console.WriteLine($"Thread #{threadNo} finished. Total: {timeMeasurements.Sum()}ms, average: {timeMeasurements.Average()}ms.");
        
            return results.ToList();
        }
        
        var watch = Stopwatch.StartNew();
        var currentSeed = actualSeed;
        for (var i = 0; i < noOfThreads; i++)
        {
            var spawnAdditionalGame = gamesPerThreadRemainder <= 0 ? 0 : 1;
            gamesPerThreadRemainder -= 1;
            var gamesToPlay = gamesPerThread + spawnAdditionalGame;
            Console.WriteLine($"Playing {gamesToPlay} games in thread #{i}");
            var threadNo = i;
            var currentSeedCopy = currentSeed;
            threads[i] = Task.Factory.StartNew(() => PlayGames(gamesToPlay, bot1Info!, bot2Info!, threadNo, currentSeedCopy));
            currentSeed += (ulong)gamesToPlay;
        }
        Task.WaitAll(threads.ToArray<Task>());
        
        var timeTaken = watch.ElapsedMilliseconds;
        
        var counter = new GameEndStatsCounter();
        threads.SelectMany(t => t.Result).ToList().ForEach(r => counter.Add(r));
        
        Console.WriteLine($"\nInitial seed used: {actualSeed}");
        Console.WriteLine($"Total time taken: {timeTaken}ms");
        Console.WriteLine("\nStats from the games played:");
        Console.WriteLine(counter.ToString());
    }
}, noOfRunsOption, threadsOption, logsOption, logFileDestination, seedOption, timeoutOption, bot1NameArgument, bot2NameArgument);

mainCommand.Invoke(args);

return returnValue;
