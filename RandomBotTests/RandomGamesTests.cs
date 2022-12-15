using TalesOfTribute.Board;

namespace RandomBotTests;

public class UnitTest1
{
    [Fact]
    public void RandomGameShouldEndWithoutErrors()
    {
        const int testAmount = 1000;

        for (var i = 0; i < testAmount; i++)
        {
            var bot1 = new RandomBot.RandomBot();
            var bot2 = new RandomBot.RandomBot();

            var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);
            var endState = game.Play();

            Assert.NotEqual(GameEndReason.INCORRECT_MOVE, endState.Reason);
        }
    }
}
