using TalesOfTribute;
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
            var bot1 = new SimpleBots.DoEverythingBot();
            var bot2 = new SimpleBots.RandomBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            counter.Add(endState);
            
            GlobalCardDatabase.Instance.Clear();
        }
        
        Assert.True(counter.P1WinPercentage > 99.0);

        _testOutputHelper.WriteLine(counter.ToString());
    }
    
    [Fact]
    public void TwoDoEverythingBotsShouldHaveSimilarWinRatio()
    {
        const int testAmount = 1000;
        GameEndStatsCounter counter = new();

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new SimpleBots.DoEverythingBot();
            var bot2 = new SimpleBots.DoEverythingBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            counter.Add(endState);
            
            GlobalCardDatabase.Instance.Clear();
        }

        Assert.True(Math.Abs(counter.P1WinPercentage - counter.P2WinPercentage) < 15.0);
        _testOutputHelper.WriteLine(counter.ToString());
    }
}
