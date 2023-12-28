using ScriptsOfTribute;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tests.utils;

public class ParserTests
{
    private Parser? _sut;
    private readonly ITestOutputHelper _testOutputHelper;

    public ParserTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    void ShouldParseAllCardsCorrectly()
    {
        _testOutputHelper.WriteLine(test_cards_config.CARDS_JSON_TWO_DECKS);
        _sut = new Parser(test_cards_config.CARDS_JSON_TWO_DECKS);
        var cards = _sut.CreateAllCards().ToList();
        Assert.Equal(3, cards.Count);

        var cardIds = cards.Select(card => card.CommonId).ToList();
        Assert.Contains(CardId.LUXURY_EXPORTS, cardIds);
        Assert.Contains(CardId.HLAALU_COUNCILOR, cardIds);
        Assert.Contains(CardId.HLAALU_KINSMAN, cardIds);
    }
}
