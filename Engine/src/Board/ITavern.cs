using TalesOfTribute.Board.Cards;

namespace TalesOfTribute;

public interface ITavern
{
    List<UniqueCard> Cards { get; set; }
    List<UniqueCard> AvailableCards { get; set; }
    void SetUp(SeededRandom rnd);
    UniqueCard Acquire(UniqueCard card);
    List<UniqueCard> GetAffordableCards(int coin);
    void ReplaceCard(UniqueCard card);
}