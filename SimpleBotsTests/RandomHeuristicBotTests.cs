using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.Board;
using Xunit.Abstractions;
using System.Text;

namespace SimpleBotsTests;

public class RandomHeuristicBotTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    private StringBuilder log = new StringBuilder();

    public RandomHeuristicBotTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void RandomHeuristicGameShouldEndWithoutErrors()
    {
        const int testAmount = 5;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new RandomHeuristicBot();
            var bot2 = new RandomBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, endBoardState) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            counter.Add(endState);

            _testOutputHelper.WriteLine(string.Join('\n', endBoardState.CompletedActions.Select(a => a.ToString())));
        }
        log.Append(counter.ToString() + System.Environment.NewLine);
        File.AppendAllText("log_with_results.txt", log.ToString());
        log.Clear();
        _testOutputHelper.WriteLine(counter.ToString());
    }
    
    [Fact]
    public void MaxPrestigeVSHeuristicRandomTest()
    {
        const int testAmount = 5;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new RandomHeuristicBot();
            var bot2 = new RandomMaximizePrestigeBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);

            counter.Add(endState);
        }
        log.Append(counter.ToString() + System.Environment.NewLine);
        File.AppendAllText("log_with_results.txt", log.ToString());
        log.Clear();
        _testOutputHelper.WriteLine(counter.ToString());
    }
}
