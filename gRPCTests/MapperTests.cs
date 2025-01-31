using ScriptsOfTribute;
using ScriptsOfTributeGRPC;

namespace gRPCTests;

public class MapperTests
{
    [Fact]
    void SerializeCard()
    {
        var br = new BoardManager(new[] { PatronId.PELIN, PatronId.RED_EAGLE, PatronId.ANSEI, PatronId.HLAALU }, 123);
        var engine_card = GlobalCardDatabase.Instance.GetCard(CardId.WAY_OF_THE_SWORD);
        var serialized = Mapper.ToUniqueCardProto(engine_card);
        Assert.Equal(engine_card.Name, serialized.Name);
        Assert.Equal("Ansei", serialized.Deck.ToString());
        Assert.Equal(engine_card.Cost, serialized.Cost);
        Assert.Equal("GAIN_POWER 1 OR GAIN_COIN 1", serialized.Effects[0]);
        Assert.Equal("", serialized.Effects[1]);
    }
}
