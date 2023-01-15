using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.Board;
using Xunit.Abstractions;

namespace SimpleBotsTests;

public class ComplicatedBotsTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ComplicatedBotsTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    /*
    [Fact]
    public void RandomHeuristicBotTests()
    {
        const int testAmount = 10;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new RandomHeuristicBot();
            var bot2 = new DoEverythingBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, endBoardState) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }
    */
    
    [Fact]
    public void HeuristicBotTests()
    {
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new HeuristicBot();
            var bot2 = new RandomBotWithRandomStateExploring();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }
    
    [Fact]
    public void MCTSBotTests()
    {
        const int testAmount = 10;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new MCTSBot();
            var bot2 = new DoEverythingBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }
}