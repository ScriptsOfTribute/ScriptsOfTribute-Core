namespace TalesOfTribute;

public class CardDatabase
{
    private List<Card> _allCards;
    public List<Card> AllCards => _allCards.Select(card => card.CreateUniqueCopy()).ToList();
    public List<Card> AllCardsWithoutUpgrades => FilterOutPreUpgradeCards(_allCards).Select(card => card.CreateUniqueCopy()).ToList();

    public CardDatabase(IEnumerable<Card> cards)
    {
        var cardsEnumerable = cards as Card[] ?? cards.ToArray();
        _allCards = cardsEnumerable.ToList();
    }

    public Card GetCard(CardId cardId)
    {
        return _allCards.First(card => card.Id == cardId).CreateUniqueCopy();
    }

    public List<Card> GetCardsByPatron(PatronId[] patrons)
    {
        var cardsFromDeck = from card in AllCardsWithoutUpgrades
                            where patrons.Contains(card.Deck) && card.Type != CardType.STARTER && card.Type != CardType.CURSE
                            select card.CreateUniqueCopy();
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
