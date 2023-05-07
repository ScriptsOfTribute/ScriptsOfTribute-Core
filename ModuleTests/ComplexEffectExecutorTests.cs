using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ModuleTests;

public class ComplexEffectExecutorTests
{
    [Fact]
    void AcquiringContractActionShouldReturnItToTavern()
    {
        var hand = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.HLAALU_KINSMAN)
        };

        var currentPlayer = new SerializedPlayer(PlayerEnum.PLAYER1, hand, new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 100, 0);
        var enemyPlayer = new SerializedPlayer(PlayerEnum.PLAYER2, new List<UniqueCard>(), new List<UniqueCard>(),
            new List<UniqueCard>(), new List<UniqueCard>(), new List<SerializedAgent>(), 0, 0, 0, 0);

        var tavernAvailableCards = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.KWAMA_EGG_MINE),
        };
        var tavernCards = new List<UniqueCard>
        {
            GlobalCardDatabase.Instance.GetCard(CardId.GOLD),
        };

        var board = new FullGameState(currentPlayer, enemyPlayer, new PatronStates(new List<Patron>()),
            tavernAvailableCards, tavernCards, 123);

        var (newState, _) = board.ApplyMove(Move.PlayCard(hand[0]));
        (newState, _) = newState.ApplyMove(Move.MakeChoice(new List<UniqueCard> { tavernAvailableCards[0] }));
        Assert.Contains(CardId.KWAMA_EGG_MINE, newState.TavernCards.Select(c => c.CommonId));
    }
}
