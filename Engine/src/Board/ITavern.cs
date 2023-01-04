namespace TalesOfTribute;

public interface ITavern
{
    List<Card> Cards { get; set; }
    List<Card> AvailableCards { get; set; }
    void DrawCards(SeededRandom rnd);
    void ShuffleBack();
    Card Acquire(Card card);
    List<Card> GetAffordableCards(int coin);
    void ReplaceCard(Card card);
}