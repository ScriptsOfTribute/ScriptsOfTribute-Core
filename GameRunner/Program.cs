
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using SimpleBotsTests;
using TalesOfTribute.AI;
using TalesOfTribute.Board;

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

var aiType = typeof(AI);
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

var logsOption = new Option<bool>(
    name: "--enable-logs",
    description: "Enable logging (to standard output by default).",
    getDefaultValue: () => false);
logsOption.AddAlias("-l");

var logFileDestination = new Option<TextWriter?>(
    name: "--log-file",
    description: "Log to file instead of standard output.",
    parseArgument: result =>
    {
        if (result.Tokens.Count == 0)
        {
            result.ErrorMessage = "No file name provided.";
            return null;
        }

        var path = result.Tokens.Single().Value;
        if (File.Exists(path))
        {
            result.ErrorMessage = "File already exists.";
            return null;
        }

        return File.CreateText(path);
    });
logFileDestination.AddAlias("-f");


Type? cachedBot = null;

Type? FindBot(string name, out string? errorMessage)
{
    errorMessage = null;

    bool findByFullName = name.Contains('.');

    if (cachedBot is not null && (findByFullName ? cachedBot.FullName : cachedBot.Name) == name)
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

    cachedBot = allBots.First(t => (findByFullName ? t.FullName : t.Name) == name);

    if (cachedBot.GetConstructor(Type.EmptyTypes) is null)
    {
        errorMessage = $"Bot {name} bot can't be instantiated as it doesn't provide a parameterless constructor.";
    }
    
    return cachedBot;
}

Type? ParseBotArg(ArgumentResult arg)
{
    if (arg.Tokens.Count != 1)
    {
        arg.ErrorMessage = "Bot name must be a single token.";
        return null;
    }

    var botType = FindBot(arg.Tokens[0].Value, out var errorMessage);
    if (errorMessage is not null)
    {
        arg.ErrorMessage = errorMessage;
        return null;
    }

    return botType!;
}

var bot1NameArgument = new Argument<Type?>(name: "bot1Name", description: "Name of the first bot.", parse: ParseBotArg);
var bot2NameArgument = new Argument<Type?>(name: "bot2Name", description: "Name of the second bot.", parse: ParseBotArg);

var mainCommand = new RootCommand("A game runner for bots.")
{
    noOfRunsOption,
    threadsOption,
    logsOption,
    logFileDestination,
    bot1NameArgument,
    bot2NameArgument,
};

int returnValue = 0;
mainCommand.SetHandler((runs, noOfThreads, enableLogs, logFileDestination, bot1Type, bot2Type) =>
{
    if (noOfThreads < 1)
    {
        Console.Error.WriteLine("ERROR: Can't use less than 1 thread.");
        returnValue = -1;
    }

    if (noOfThreads == 1)
    {
        Console.WriteLine($"Running {runs} games: {bot1Type!.Name} vs {bot2Type!.Name}:\n");

        var counter = new GameEndStatsCounter();

        var timeMeasurements = new long[runs];

        var granularWatch = new Stopwatch();
        for (var i = 0; i < runs; i++)
        {
            var bot1 = (AI?)Activator.CreateInstance(bot1Type);
            var bot2 = (AI?)Activator.CreateInstance(bot2Type);

            granularWatch.Reset();
            granularWatch.Start();
            var game = new TalesOfTribute.AI.TalesOfTribute(bot1!, bot2!)
            {
                LoggerEnabled = enableLogs
            };
            if (logFileDestination is not null)
            {
                game.LogTarget = logFileDestination;
            }
            var (endReason, _) = game.Play();
            granularWatch.Stop();

            var gameTimeTaken = granularWatch.ElapsedMilliseconds;
            timeMeasurements[i] = gameTimeTaken;

            counter.Add(endReason);
        }

        Console.WriteLine($"Total time taken: {timeMeasurements.Sum()}ms");
        Console.WriteLine($"Average time per game: {timeMeasurements.Average()}ms");
        Console.WriteLine("\nStats from the games played:");
        Console.WriteLine(counter.ToString());    
    }
    else
    {
        if (enableLogs)
        {
            Console.Error.WriteLine("ERROR: Logs are not supported with multi-threading.");
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

        List<EndGameState> PlayGames(int amount, Type bot1T, Type bot2T, int threadNo)
        {
            var results = new EndGameState[amount];
            var timeMeasurements = new long[amount];
            var watch = new Stopwatch();
            for (var i = 0; i < amount; i++)
            {
                var bot1 = (AI?)Activator.CreateInstance(bot1T);
                var bot2 = (AI?)Activator.CreateInstance(bot2T);
                watch.Reset();
                watch.Start();
                var game = new TalesOfTribute.AI.TalesOfTribute(bot1!, bot2!);
                var (endReason, _) = game.Play();
                watch.Stop();
                results[i] = endReason;
                timeMeasurements[i] = watch.ElapsedMilliseconds;
            }

            Console.WriteLine($"Thread #{threadNo} finished. Total: {timeMeasurements.Sum()}ms, average: {timeMeasurements.Average()}.");

            return results.ToList();
        }

        var watch = Stopwatch.StartNew();
        for (var i = 0; i < noOfThreads; i++)
        {
            var spawnAdditionalGame = gamesPerThreadRemainder <= 0 ? 0 : 1;
            gamesPerThreadRemainder -= 1;
            var gamesToPlay = gamesPerThread + spawnAdditionalGame;
            Console.WriteLine($"Playing {gamesToPlay} games in thread #{i}");
            threads[i] = Task.Factory.StartNew(() => PlayGames(gamesToPlay, bot1Type!, bot2Type!, i));
        }
        Task.WaitAll(threads.ToArray<Task>());

        var timeTaken = watch.ElapsedMilliseconds;

        var counter = new GameEndStatsCounter();
        threads.SelectMany(t => t.Result).ToList().ForEach(r => counter.Add(r));

        Console.WriteLine($"\nTotal time taken: {timeTaken}ms");
        Console.WriteLine("\nStats from the games played:");
        Console.WriteLine(counter.ToString());
    }
}, noOfRunsOption, threadsOption, logsOption, logFileDestination, bot1NameArgument, bot2NameArgument);

mainCommand.Invoke(args);

return returnValue;
