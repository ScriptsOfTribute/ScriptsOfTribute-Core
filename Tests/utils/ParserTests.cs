using TalesOfTribute;

namespace Tests.utils;

public class ParserTests
{
    private Parser? sut;

    [Fact]
    public void ShouldParseCardsForCorrectDeck()
    {
        sut = new Parser(test_cards_config.CARDS_JSON_TWO_DECKS);

        var cards = sut.GetCardsByDeck(new[] { "Hlaalu" });
        Assert.Equal(2, cards.Count);
        Assert.All(cards, card => Assert.Equal(PatronEnum.Hlaalu, card.Deck));
    }

    [Fact]
    public void ShouldCorrectlyFilterOutPreUpgradeCard()
    {
        sut = new Parser(test_cards_config.CARDS_JSON_PREUPGRADE_CARDS);

        var cards = sut.GetCardsByDeck(new[] { "Hlaalu" });
        Assert.Equal(2, cards.Count);
        Assert.DoesNotContain(5, cards.Select(card => card.InstanceID));
    }
}
