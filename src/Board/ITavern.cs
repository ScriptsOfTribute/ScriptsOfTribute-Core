namespace TalesOfTribute;

public interface ITavern
{
    List<Card> Cards { get; set; }
    List<Card> AvailableCards { get; set; }
    void DrawCards();
    void ShuffleBack();
    Card Acquire(CardId card);
    List<Card> GetAffordableCards(int coin);
    void ReplaceCard(CardId card);
}