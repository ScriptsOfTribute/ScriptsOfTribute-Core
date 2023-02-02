using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute;

public interface ITavern
{
    List<UniqueCard> Cards { get; set; }
    List<UniqueCard> AvailableCards { get; set; }
    void SetUp(SeededRandom rnd);
    List<UniqueCard> GetAffordableCards(int coin);
    void ReplaceCard(UniqueCard card);
    public int RemoveCard(UniqueCard card);
    public void DrawAt(int index);
}