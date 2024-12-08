using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using Tests.utils;

namespace ScriptsOfTributeGRPC;

public class MapperTests
{
    private CardDatabase database;

    public MapperTests()
    {
        var parser = new Parser(test_cards_config.CARDS_JSON_TWO_DECKS);
        database = new CardDatabase(parser.CreateAllCards());
    }

    [Fact]
    void MapCardObjects()
    {
        var cards = database.GetCardsByPatron(new[] { ScriptsOfTribute.PatronId.HLAALU });
        foreach (var card in cards)
        {

            Assert.Equal(card.UniqueId.Value, 1);
        }
    }
}