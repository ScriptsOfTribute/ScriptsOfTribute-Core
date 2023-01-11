
using System.CommandLine;

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
}, noOfRunsOption, bot1NameArgument, bot2NameArgument);

mainCommand.Invoke(args);