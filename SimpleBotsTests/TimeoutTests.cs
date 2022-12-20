using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.Board;

namespace SimpleBotsTests;

public class TimeoutTests
{
    [Fact]
    void MoveShouldCorrectlyTimeout()
    {
        var bot1 = new RandomBot();
        var bot2 = new MoveTimeoutBot();
        var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);

        var result = game.Play();
        
        Assert.Equal(GameEndReason.MOVE_TIMEOUT, result.Reason);
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
    }
    
    [Fact]
    void TurnShouldCorrectlyTimeout()
    {
        var bot1 = new RandomBot();
        var bot2 = new TurnTimeoutBot();
        var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);

        var result = game.Play();
        
        Assert.Equal(GameEndReason.TURN_TIMEOUT, result.Reason);
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
    }
    
    [Fact]
    void PatronShouldCorrectlyTimeout()
    {
        var bot1 = new RandomBot();
        var bot2 = new PatronSelectionTimeoutBot();
        var game = new TalesOfTribute.AI.TalesOfTribute(bot1, bot2);

        var result = game.Play();
        
        Assert.Equal(GameEndReason.PATRON_SELECTION_TIMEOUT, result.Reason);
        Assert.Equal(PlayerEnum.PLAYER1, result.Winner);
    }
}
