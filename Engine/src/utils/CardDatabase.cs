using TalesOfTribute.Board.Cards;

namespace TalesOfTribute;

public class CardDatabase
{
    private List<Card> _allCards;
    public List<UniqueCard> AllCards => _allCards.Select(card => card.CreateUniqueCopy()).ToList();

    public CardDatabase(IEnumerable<Card> cards)
    {
        var cardsEnumerable = cards as UniqueCard[] ?? cards.ToArray();
        _allCards = cardsEnumerable.ToList();
        if (!_allCards.Select(c => c.CommonId).Contains(CardId.UNKNOWN))
        {
            _allCards.Add(new Card("Unknown", PatronId.TREASURY, CardId.UNKNOWN, 0, CardType.ACTION, -1, new ComplexEffect? []{ null, null, null, null }, 0, null, false, 1));
        }
    }

    public UniqueCard GetCard(CardId cardId)
    {
        UniqueCard card = _allCards.First(card => card.CommonId == cardId).CreateUniqueCopy();
        return card;
    }

    public List<UniqueCard> GetCardsByPatron(PatronId[] patrons)
    {
        var allCardsGrouped = from card in _allCards
            where patrons.Contains(card.Deck) && card.Type != CardType.STARTER && card.Type != CardType.CURSE &&
                  card.CommonId != CardId.UNKNOWN
            select Enumerable.Range(0, card.Copies).Select(_ => card.CreateUniqueCopy()).ToList();

        return allCardsGrouped.SelectMany(c => c).ToList();
    }
}
