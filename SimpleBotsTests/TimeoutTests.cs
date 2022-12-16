using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.Board;

namespace RandomBotTests;

public class TimeoutTests
{
    [Fact]
    async void MoveShouldCorrectlyTimeout()
    {
        var bot1 = new RandomBot();
        var bot2 = new MoveTimeoutBot();
        var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);

        var result = await game.Play();
        
        Assert.Equal(GameEndReason.TIMEOUT, result.Reason);
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
    }
}
