namespace TalesOfTribute;

public class CardDatabase
{
    private List<Card> _allCards;
    private List<Card> _allCardsInPlay;
    public List<Card> AllCards => _allCards.Select(card => card.CreateUniqueCopy()).ToList();
    public List<Card> AllCardsWithoutUpgrades => FilterOutPreUpgradeCards(_allCards).Select(card => card.CreateUniqueCopy()).ToList();

    public CardDatabase(IEnumerable<Card> cards)
    {
        var cardsEnumerable = cards as Card[] ?? cards.ToArray();
        _allCards = cardsEnumerable.ToList();
        _allCardsInPlay = new List<Card>();
    }

    public Card GetCard(CardId cardId)
    {
        Card card = _allCards.First(card => card.CommonId == cardId).CreateUniqueCopy();
        _allCardsInPlay.Add(card);
        return card;
    }

    public Card GetExistingCard(UniqueId id)
    {
        return _allCardsInPlay.First(card => card.UniqueId == id);
    }

    public List<Card> GetCardsByPatron(PatronId[] patrons)
    {
        var cardsFromDeck = from card in AllCardsWithoutUpgrades
                            where patrons.Contains(card.Deck) && card.Type != CardType.STARTER && card.Type != CardType.CURSE
                            select card.CreateUniqueCopy();
        _allCardsInPlay.AddRange(cardsFromDeck.ToList());
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
               where !families.Contains(card.CommonId)
               select card;
    }
}
