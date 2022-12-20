using TalesOfTribute;
using TalesOfTribute.Board;
using Xunit.Abstractions;

namespace RandomBotTests;

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
        const int testAmount = 1000;
        var drawCount = 0;

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new SimpleBots.RandomBot();
            var bot2 = new SimpleBots.RandomBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
            Assert.NotEqual(GameEndReason.MOVE_TIMEOUT, endState.Reason);

            if (endState.Reason == GameEndReason.TURN_LIMIT_EXCEEDED)
            {
                drawCount++;
            }
            
            GlobalCardDatabase.Instance.Clear();
        }

        _testOutputHelper.WriteLine($"Final amount of draws: {drawCount}/{testAmount} games ({100.0*drawCount/testAmount}%)");
    }
}
