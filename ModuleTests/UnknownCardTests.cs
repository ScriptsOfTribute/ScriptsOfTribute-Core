using TalesOfTribute;
using TalesOfTribute.Board.Cards;
using TalesOfTribute.Serializers;

namespace ModuleTests;

public class UnknownCardTests
{
    [Fact]
    void SimulatingBuyFromTavernShouldInsertUnknownCardThatIsUnavailableToBuy()
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
        
        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            tavernAvailableCards, tavernCards, 123);

        var (newState, possibleMoves) = board.ApplyState(Move.BuyCard(tavernAvailableCards[0]));
        Assert.Equal(CardId.UNKNOWN, newState.TavernAvailableCards[0].CommonId);
        Assert.Equal(0, possibleMoves.Where(m => m.Command == CommandEnum.BUY_CARD).Count(m => (m as SimpleCardMove)!.Card.CommonId == CardId.UNKNOWN));
    }
    
    [Fact]
    void SimulatingDrawFromTavernShouldInsertUnknownCardThatIsUnavailableToPlayButCanBeDiscarded()
    {
        var drawpile = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.PROPHESY)
        };

        var hand = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.HARVEST_SEASON),
            GlobalCardDatabase.Instance.GetCard(CardId.RAGPICKER),
        };
        
        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, drawpile,
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var board = new SerializedBoard(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            new List<UniqueCard>(), new List<UniqueCard>(), 123);

        var (newState, possibleMoves) = board.ApplyState(Move.PlayCard(hand[0]));
        Assert.Contains(CardId.UNKNOWN, newState.CurrentPlayer.Hand.Select(c => c.CommonId));
        Assert.Equal(0, possibleMoves.Where(m => m.Command == CommandEnum.PLAY_CARD).Count(m => (m as SimpleCardMove)!.Card.CommonId == CardId.UNKNOWN));
        Assert.Equal(1, possibleMoves.Count(m => m.Command == CommandEnum.PLAY_CARD));

        (newState, _) = newState.ApplyState(Move.PlayCard(hand[1]));
        Assert.Contains(CardId.UNKNOWN, newState.PendingChoice!.PossibleCards.Select(c => c.CommonId));

        (newState, _) =
            newState.ApplyState(
                Move.MakeChoice(newState.PendingChoice!.PossibleCards.First(c => c.CommonId == CardId.UNKNOWN)));
        
        Assert.Empty(newState.CurrentPlayer.Hand);
    }
}
