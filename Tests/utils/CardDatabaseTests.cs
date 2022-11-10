using TalesOfTribute;

namespace Tests.utils;

public class CardDatabaseTests
{
    private CardDatabase? sut;

    [Fact]
    public void ShouldParseCardsForCorrectDeck()
    {
        var parser = new Parser(test_cards_config.CARDS_JSON_TWO_DECKS);
        sut = new CardDatabase(parser.CreateAllCards());

        var cards = sut.GetCardsByPatron(new[] { PatronId.HLAALU });
        Assert.Equal(2, cards.Count);
        Assert.All(cards, card => Assert.Equal(PatronId.HLAALU, card.Deck));
    }

    [Fact]
    public void ShouldCorrectlyFilterOutPreUpgradeCard()
    {
        var parser = new Parser(test_cards_config.CARDS_JSON_PREUPGRADE_CARDS);
        sut = new CardDatabase(parser.CreateAllCards());

        var cards = sut.GetCardsByPatron(new[] { PatronId.HLAALU });
        Assert.Equal(2, cards.Count);
        Assert.DoesNotContain((CardId)5, cards.Select(card => card.Id));
    }
}
