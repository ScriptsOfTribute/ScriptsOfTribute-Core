
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;
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

AI? ParseBotArg(ArgumentResult arg)
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

    return (AI)Activator.CreateInstance(botType!);
}

var bot1NameArgument = new Argument<AI?>(name: "bot1Name", description: "Name of the first bot.", parse: ParseBotArg);
var bot2NameArgument = new Argument<AI?>(name: "bot2Name", description: "Name of the second bot.", parse: ParseBotArg);

var mainCommand = new RootCommand("A game runner for bots.")
{
    noOfRunsOption,
    bot1NameArgument,
    bot2NameArgument
};

mainCommand.SetHandler((runs, bot1Name, bot2Name) =>
{
    Console.WriteLine($"Runs: {runs}, bot1name: {bot1Name}, bot2name: {bot2Name}");

    var game = new TalesOfTribute.AI.TalesOfTribute(bot1Name, bot2Name);
    var (endReason, endState) = game.Play();

    Console.WriteLine($"Game ended: {endReason.Reason}");
}, noOfRunsOption, bot1NameArgument, bot2NameArgument);

return mainCommand.Invoke(args);
