using TalesOfTribute;
using TalesOfTribute.Board.Cards;
using TalesOfTribute.Serializers;

namespace ModuleTests;

public class GameStateTests
{
    [Fact]
    void GameStateShouldIntroduceUnknownCards()
    {
        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 100, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var tavernAvailableCards = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
        };
        var tavernCards = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY),
        };
        
        var board = new GameState(new FullGameState(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            tavernAvailableCards, tavernCards, 123));

        var (newState, possibleMoves) = board.ApplyState(Move.BuyCard(tavernAvailableCards[0]));
        Assert.Equal(CardId.UNKNOWN, newState.TavernAvailableCards[0].CommonId);
        Assert.Equal(0, possibleMoves.Where(m => m.Command == CommandEnum.BUY_CARD).Count(m => (m as SimpleCardMove)!.Card.CommonId == CardId.UNKNOWN));
    }

    [Fact]
    void GameStateShouldNotAllowToPlayAsEnemy()
    {
        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 100, 0);
        var enemyHand = new List<UniqueCard> { GlobalCardDatabase.Instance.GetCard(CardId.GOLD) };
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, enemyHand, new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new GameState(new FullGameState(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123));

        var (newState, possibleMoves) = board.ApplyState(Move.EndTurn());
        var ex = Assert.Throws<EngineException>(() => newState.ApplyState(Move.PlayCard(enemyHand[0])));
        Assert.Equal("You can't simulate any more moves as you've ended your turn.", ex.Message);
    }

    [Fact]
    void SeededGameStateShouldNotIntroduceUnknownCards()
    {
        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 100, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var tavernAvailableCards = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
        };
        var tavernCards = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY),
        };
        
        var board = new GameState(new FullGameState(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            tavernAvailableCards, tavernCards, 123));

        var (newState, possibleMoves) = board.ApplyState(Move.BuyCard(tavernAvailableCards[0]), 123);
        Assert.Equal(CardId.UNKNOWN, newState.TavernAvailableCards[0].CommonId);
        Assert.Equal(0, possibleMoves.Where(m => m.Command == CommandEnum.BUY_CARD).Count(m => (m as SimpleCardMove)!.Card.CommonId == CardId.UNKNOWN));
    }

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

        var (newState, possibleMoves) = board.ApplyState(Move.EndTurn(), 123);
        (newState, possibleMoves) = newState.ApplyState(Move.PlayCard(enemyHand[0]));
        Assert.Equal(1, newState.CurrentPlayer.Coins);
    }
}
