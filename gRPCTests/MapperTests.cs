using System;
using System.Globalization;
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

    public static bool FormatEnumDifference(string expected, string actual)
    {
        var formattedExpected = expected.ToLower().Replace("_", "");
        var formattedActual = actual.ToLower().Replace("_", "");

        return formattedExpected == formattedActual;
    }

    [Fact]
    void MapCardObjects()
    {
        var cards = database.GetCardsByPatron(new[] { ScriptsOfTribute.PatronId.HLAALU });
        TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
        foreach (var card in cards)
        {
            var grpcCard = Mapper.MapCard(card);
            Console.Write("\n");
            Assert.Equal(card.UniqueId.Value, grpcCard.UniqueId);
            Assert.Equal(card.Name, grpcCard.Name);
            Assert.True(
                FormatEnumDifference(Enum.GetName(typeof(CardId), grpcCard.CommonId)!,Enum.GetName(typeof(ScriptsOfTribute.CardId), card.CommonId)!)
            );
        }
    }
}