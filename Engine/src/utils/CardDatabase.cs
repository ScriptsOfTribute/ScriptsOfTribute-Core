using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute;

public class CardDatabase
{
    private List<Card> _allCards;
    public List<UniqueCard> AllCards => _allCards.Select(card => card.CreateUniqueCopy()).ToList();

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

    public List<UniqueCard> GetCardsByPatron(PatronId[] patrons, CardId[] starterCardsId, params CardId[] excludeCards)
    {
        var allCardsGrouped = from card in _allCards
            where patrons.Contains(card.Deck) && !starterCardsId.Contains(card.CommonId) && !excludeCards.Contains(card.CommonId) && card.Type != CardType.CURSE
            select Enumerable.Range(0, card.Copies).Select(_ => card.CreateUniqueCopy()).ToList();

        return allCardsGrouped.SelectMany(c => c).ToList();
    }
}
