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
        var drawCount = 0;
        var doEverythingWins = 0;

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new SimpleBots.DoEverythingBot();
            var bot2 = new SimpleBots.RandomBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            if (endState.Reason == GameEndReason.TURN_LIMIT_EXCEEDED)
            {
                drawCount++;
            }

            if (endState.Winner == PlayerEnum.PLAYER1)
            {
                doEverythingWins++;
            }
            
            GlobalCardDatabase.Instance.Clear();
        }

        _testOutputHelper.WriteLine($"Final amount of draws: {drawCount}/{testAmount} games ({100.0*drawCount/testAmount}%)");
        var doEverythingWinPercentage = 100.0 * doEverythingWins / testAmount;
        _testOutputHelper.WriteLine($"Final amount of do everything bot wins: {doEverythingWins}/{testAmount} games ({doEverythingWinPercentage}%)");
        
        Assert.True(doEverythingWinPercentage > 99.0);
    }
    
    [Fact]
    public void TwoDoEverythingBotsShouldHaveSimilarWinRatio()
    {
        const int testAmount = 1000;
        var drawCount = 0;
        var p1Wins = 0;

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new SimpleBots.DoEverythingBot();
            var bot2 = new SimpleBots.DoEverythingBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            if (endState.Reason == GameEndReason.TURN_LIMIT_EXCEEDED)
            {
                drawCount++;
            }

            if (endState.Winner == PlayerEnum.PLAYER1)
            {
                p1Wins++;
            }
            
            GlobalCardDatabase.Instance.Clear();
        }

        _testOutputHelper.WriteLine($"Final amount of draws: {drawCount}/{testAmount} games ({100.0*drawCount/testAmount}%)");
        var p1WinPercentage = 100.0 * p1Wins / testAmount;
        _testOutputHelper.WriteLine($"Final amount of P1 wins: {p1Wins}/{testAmount} games ({p1WinPercentage}%)");
        var p2Wins = testAmount - p1Wins - drawCount;
        var p2WinPercentage = 100.0 * p2Wins / testAmount;
        _testOutputHelper.WriteLine($"Final amount of P2 wins: {p2Wins}/{testAmount} games ({p2WinPercentage}%)");

        Assert.True(Math.Abs(p1WinPercentage - p2WinPercentage) < 15.0);
    }
}
