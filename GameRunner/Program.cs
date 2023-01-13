
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using SimpleBotsTests;
using TalesOfTribute.AI;

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

var aiType = typeof(AI);
List<Type> allBots = currentDirectory.GetFiles("*.dll").Select(f => f.FullName)
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

Type? cachedBot = null;

Type? FindBot(string name, out string? errorMessage)
{
    errorMessage = null;

    if (cachedBot is not null && cachedBot.Name == name)
    {
        return cachedBot;
    }

    var botCount = allBots.Count(t => t.Name == name);

    if (botCount == 0)
    {
        errorMessage = $"Bot {name} not found in any DLLs.";
        return null;
    }

    if (botCount > 1)
    {
        errorMessage = "More than one bots with the same name found. Please, specify full name of the target bot: <namespace>.Name";
        return null;
    }

    cachedBot = allBots.First(t => t.Name == name);

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
    bot1NameArgument,
    bot2NameArgument
};

int returnValue = 0;
mainCommand.SetHandler((runs, threads, bot1Type, bot2Type) =>
{
    if (threads < 1)
    {
        Console.Error.WriteLine("Can't use less than 0 threads.");
        returnValue = -1;
    }

    if (threads == 1)
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
            var game = new TalesOfTribute.AI.TalesOfTribute(bot1!, bot2!);
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
        
    }
}, noOfRunsOption, threadsOption, bot1NameArgument, bot2NameArgument);

mainCommand.Invoke(args);

return returnValue;
