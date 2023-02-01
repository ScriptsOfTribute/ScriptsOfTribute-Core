using TalesOfTribute.Board;
using Xunit.Abstractions;

namespace SimpleBotsTests;

public class DoEverythingBotTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DoEverythingBotTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void DoEverythingBotShouldHaveAdvantageOverRandom()
    {
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new SimpleBots.RandomWithoutEndTurnBot();
            var bot2 = new SimpleBots.RandomBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }
        
        // Winrate for DoEverythingBot is expected to be >99%
        _testOutputHelper.WriteLine(counter.ToString());
    }
    
    [Fact]
    public void TwoDoEverythingBotsShouldHaveSimilarWinRatio()
    {
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new SimpleBots.RandomWithoutEndTurnBot();
            var bot2 = new SimpleBots.RandomWithoutEndTurnBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var (endState, _) = game.Play();

            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.INTERNAL_ERROR, endState.Reason);

            counter.Add(endState);
        }

        // Winrate is expected to be around 55-45
        _testOutputHelper.WriteLine(counter.ToString());
    }
}