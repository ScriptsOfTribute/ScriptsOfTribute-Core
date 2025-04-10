using ScriptsOfTribute;

namespace Tests.utils;

public class CardDatabaseTests
{
    private CardDatabase? sut;

    [Fact]
    public void ShouldParseCardsForCorrectDeck()
    {
        var parser = new Parser(test_cards_config.CARDS_JSON_TWO_DECKS);
        sut = new CardDatabase(parser.CreateAllCards());

        var cards = sut.GetCardsByPatron(new[] { PatronId.HLAALU }, new[] {CardId.GOODS_SHIPMENT});
        Assert.Equal(3, cards.Count);
        Assert.All(cards, card => Assert.Equal(PatronId.HLAALU, card.Deck));
    }

    [Fact (Skip = "I don't see where we filter out these basic cards. Skipping till I figure this out.")]
    public void ShouldCorrectlyFilterOutPreUpgradeCard()
    {
        var parser = new Parser(test_cards_config.CARDS_JSON_PREUPGRADE_CARDS);
        sut = new CardDatabase(parser.CreateAllCards());

        var cards = sut.GetCardsByPatron(new[] { PatronId.HLAALU }, new[] {CardId.GOODS_SHIPMENT});
        Assert.Equal(2, cards.Count);
        Assert.DoesNotContain((CardId)5, cards.Select(card => card.CommonId));
    }

    [Fact]
    public void ShouldAlwaysReturnUniqueCards()
    {
        var parser = new Parser(test_cards_config.CARDS_JSON_PREUPGRADE_CARDS);
        sut = new CardDatabase(parser.CreateAllCards());

        var card1 = sut.GetCardsByPatron(new[] { PatronId.HLAALU }, new[] {CardId.GOODS_SHIPMENT}).First();
        var card2 = sut.GetCardsByPatron(new[] { PatronId.HLAALU }, new[] {CardId.GOODS_SHIPMENT}).First();

        Assert.NotEqual(card1.UniqueId, card2.UniqueId);
    }
}
