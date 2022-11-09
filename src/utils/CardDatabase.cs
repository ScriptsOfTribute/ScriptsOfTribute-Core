namespace TalesOfTribute;

public class CardDatabase
{
    public List<Card> AllCards { get; }
    public List<Card> AllCardsWithoutUpgrades { get; }
    
    public CardDatabase(IEnumerable<Card> cards)
    {
        var cardsEnumerable = cards as Card[] ?? cards.ToArray();
        AllCards = cardsEnumerable.ToList();
        AllCardsWithoutUpgrades = FilterOutPreUpgradeCards(cardsEnumerable).ToList();
    }

    public Card GetCard(CardId cardId)
    {
        return AllCards.First(card => card.Id == cardId);
    }

    public List<Card> GetCardsByPatron(PatronId[] patrons)
    {
        var cardsFromDeck = from card in AllCardsWithoutUpgrades where patrons.Contains(card.Deck) select card;
        return cardsFromDeck.ToList();
    }
    
    // TODO: Add ability for user to configure which card he wants to keep.
    // For now, we will keep cards after upgrade.
    private static IEnumerable<Card> FilterOutPreUpgradeCards(IEnumerable<Card> cards)
    {
        var cardsEnumerable = cards.ToList();
        var families = cardsEnumerable
            .Where(card => card.Family != null)
            .Select(card => card.Family)
            .Distinct();
        // Pre-upgrade cards have the same ID as the family they are in.
        return from card in cardsEnumerable
            where !families.Contains(card.Id)
            select card;
    }
}
