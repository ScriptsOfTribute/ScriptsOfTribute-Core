using ScriptsOfTribute;
using ScriptsOfTributeGRPC;

namespace gRPCTests;

public class MapperTests
{
    [Fact]
    void SerializeCard()
    {
        var engine_card = GlobalCardDatabase.Instance.GetCard(CardId.WAY_OF_THE_SWORD);
        var serialized = Mapper.ToUniqueCardProto(engine_card);
        Assert.Equal(engine_card.Name, serialized.Name);
        Assert.Equal("Ansei", serialized.Deck.ToString());
        Assert.Equal(engine_card.Cost, serialized.Cost);
        Assert.Equal("GAIN_POWER 1 OR GAIN_COIN 1", serialized.Effects[0]);
        Assert.Equal("", serialized.Effects[1]);
    }
}
