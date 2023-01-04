using TalesOfTribute.Board.Cards;

namespace TalesOfTribute;

public interface ITavern
{
    List<UniqueCard> Cards { get; set; }
    List<UniqueCard> AvailableCards { get; set; }
    void DrawCards(SeededRandom rnd);
    void ShuffleBack();
    UniqueCard Acquire(UniqueCard card);
    List<UniqueCard> GetAffordableCards(int coin);
    void ReplaceCard(UniqueCard card);
}