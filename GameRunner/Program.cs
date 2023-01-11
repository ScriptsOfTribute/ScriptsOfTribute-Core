
using System.CommandLine;
using System.Reflection;
using TalesOfTribute.AI;

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

var aiType = typeof(AI);
Console.WriteLine("Listing files:");
Console.WriteLine(string.Join(',',currentDirectory.GetFiles("*.dll").Select(f => f.FullName)));
var allBots = currentDirectory.GetFiles("*.dll").Select(f => f.FullName)
    .Select(Assembly.LoadFile)
    .SelectMany(a => a.GetTypes())
    .Where(t => aiType.IsAssignableFrom(t) && !t.IsInterface)
    .ToList();

var noOfRunsOption = new Option<int>(
    name: "--runs",
    description: "Number of games to run.",
    getDefaultValue: () => 1);
noOfRunsOption.AddAlias("-n");

var bot1NameArgument = new Argument<string>(name: "bot1Name", description: "Name of the first bot.");
var bot2NameArgument = new Argument<string>(name: "bot2Name", description: "Name of the second bot.");

var mainCommand = new RootCommand("A game runner for bots.")
{
    noOfRunsOption,
    bot1NameArgument,
    bot2NameArgument
};

mainCommand.SetHandler((runs, bot1Name, bot2Name) =>
{
    Console.WriteLine($"Runs: {runs}, bot1name: {bot1Name}, bot2name: {bot2Name}");

    Console.WriteLine("All bots:");
    Console.WriteLine(string.Join(',', allBots.Select(t => t.FullName)));
    
    var bot1 = allBots.First(t => t.Name == bot1Name);
    var bot2 = allBots.First(t => t.Name == bot2Name);

    Console.WriteLine("Bots loaded successfully!");

}, noOfRunsOption, bot1NameArgument, bot2NameArgument);

mainCommand.Invoke(args);