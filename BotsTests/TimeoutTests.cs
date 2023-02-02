using Bots;
using ScriptsOfTribute;
using ScriptsOfTribute.Board;

namespace BotsTests;

public class TimeoutTests
{

    [Fact]
    void TurnShouldCorrectlyTimeout()
    {
        var bot1 = new RandomBot();
        // This bot should timeout, because he has to wait 1 second between each move.
        // That means he can make 3 moves, but in the first turn there are always 5 possible cards to play,
        // and he tries to make all the moves before ending turn.
        var bot2 = new TurnTimeoutBot();
        var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1, bot2)
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        var (result, _) = game.Play();

        Assert.Equal(GameEndReason.TURN_TIMEOUT, result.Reason);
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
    }

    [Fact]
    void PatronShouldCorrectlyTimeout()
    {
        var bot1 = new RandomBot();
        var bot2 = new PatronSelectionTimeoutBot();
        var game = new ScriptsOfTribute.AI.ScriptsOfTribute(bot1, bot2)
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        var (result, _) = game.Play();

        Assert.Equal(GameEndReason.PATRON_SELECTION_TIMEOUT, result.Reason);
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
    }
}
