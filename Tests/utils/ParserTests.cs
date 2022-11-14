using TalesOfTribute;

namespace Tests.utils;

public class ParserTests
{
    private Parser? _sut;

    [Fact]
    void ShouldParseAllCardsCorrectly()
    {
        _sut = new Parser(test_cards_config.CARDS_JSON_TWO_DECKS);
        var cards = _sut.CreateAllCards().ToList();
        Assert.Equal(3, cards.Count);

        var cardIds = cards.Select(card => card.Id).ToList();
        Assert.Contains(CardId.LUXURY_EXPORTS, cardIds);
        Assert.Contains(CardId.HLAALU_COUNCILOR, cardIds);
        Assert.Contains(CardId.HLAALU_KINSMAN, cardIds);
    }
}
