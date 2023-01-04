using TalesOfTribute.Board.Cards;

namespace TalesOfTribute;

public class CardDatabase
{
    private List<Card> _allCards;
    public List<UniqueCard> AllCards => _allCards.Select(card => card.CreateUniqueCopy()).ToList();
    public List<UniqueCard> AllCardsWithoutUpgrades => FilterOutPreUpgradeCards(_allCards).Select(card => card.CreateUniqueCopy()).ToList();

    public CardDatabase(IEnumerable<Card> cards)
    {
        var cardsEnumerable = cards as UniqueCard[] ?? cards.ToArray();
        _allCards = cardsEnumerable.ToList();
    }

    public UniqueCard GetCard(CardId cardId)
    {
        UniqueCard card = _allCards.First(card => card.CommonId == cardId).CreateUniqueCopy();
        return card;
    }

    public List<UniqueCard> GetCardsByPatron(PatronId[] patrons)
    {
        List<UniqueCard> cardsFromDeck = (
            from card in AllCardsWithoutUpgrades
            where patrons.Contains(card.Deck) && card.Type != CardType.STARTER && card.Type != CardType.CURSE
            select card.CreateUniqueCopy()
        ).ToList();
        return cardsFromDeck;
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
