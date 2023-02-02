using Bots;
using ScriptsOfTribute.Board;
using Xunit.Abstractions;

namespace BotsTests;

public class RandomGamesTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RandomGamesTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void RandomGameShouldEndWithoutErrors()
    {
        const int testAmount = 500;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new RandomBot();
            var bot2 = new RandomBot();

            var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1, bot2);
            var (endState, endBoardState) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);

            _testOutputHelper.WriteLine(string.Join('\n', endBoardState.CompletedActions.Select(a => a.ToString())));
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }

    [Fact]
    public void RandomBotWithRandomStateExploringTests()
    {
        const int testAmount = 500;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new RandomBotWithRandomStateExploring();
            var bot2 = new RandomBotWithRandomStateExploring();

            var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }

    [Fact]
    public void MaxPrestigeTest()
    {
        const int testAmount = 500;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new MaxPrestigeBot();
            var bot2 = new RandomWithoutEndTurnBot();

            var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }

    [Fact]
    public void WinByPatronFavours()
    {
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new PatronFavorsBot();
            var bot2 = new RandomWithoutEndTurnBot();

            var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }

    [Fact]
    public void MaximizeAgentsBot()
    {
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new MaxAgentsBot();
            var bot2 = new RandomWithoutEndTurnBot();

            var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            if (endState.Reason == GameEndReason.INCORRECT_MOVE)
            {
                _testOutputHelper.WriteLine(endState.AdditionalContext);
            }
            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.TURN_TIMEOUT, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        _testOutputHelper.WriteLine(counter.ToString());
    }
}