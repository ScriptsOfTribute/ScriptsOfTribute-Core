using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ModuleTests;

public class GameStateTests
{
    [Fact]
    void SeededGameStateShouldAllowToPlayAsEnemy()
    {
        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 100, 0);
        var enemyHand = new List<UniqueCard> { GlobalCardDatabase.Instance.GetCard(CardId.GOLD) };
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, enemyHand, new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new GameState(new FullGameState(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123));

        var (newState, possibleMoves) = board.ApplyMove(Move.EndTurn(), 123);
        (newState, possibleMoves) = newState.ApplyMove(Move.PlayCard(enemyHand[0]));
        Assert.Equal(1, newState.CurrentPlayer.Coins);
    }
}
