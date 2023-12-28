using Newtonsoft.Json.Linq;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;
using Xunit.Abstractions;

namespace Tests.utils;

public class JSONSerializeTests
{

    private readonly ITestOutputHelper _testOutputHelper;

    public JSONSerializeTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    void SerializePatronStates()
    {
        Patron p1 = new Ansei();
        p1.FavoredPlayer = PlayerEnum.PLAYER1;
        Patron p2 = new DukeOfCrows();
        p2.FavoredPlayer = PlayerEnum.PLAYER2;
        Patron p3 = new Pelin();
        p3.FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;
        Patron p4 = new Rajhin();
        p4.FavoredPlayer = PlayerEnum.PLAYER1;
        PatronStates ps = new PatronStates(new List<Patron> { p1, p2, p3, p4 });

        JObject expected = new JObject
        {
            {"ANSEI", "PLAYER1"},
            {"DUKE_OF_CROWS", "PLAYER2"},
            {"PELIN", "NO_PLAYER_SELECTED"},
            {"RAJHIN", "PLAYER1"},
        };

        Assert.Equal(ps.SerializeObject(), expected);
    }

    [Fact]
    void SerializeUniqueCard()
    {
        var card1 = GlobalCardDatabase.Instance.GetCard(CardId.HLAALU_KINSMAN);
        var card2 = GlobalCardDatabase.Instance.GetCard(CardId.WAY_OF_THE_SWORD);
        var card3 = GlobalCardDatabase.Instance.GetCard(CardId.GRAND_LARCENY);
        var card4 = GlobalCardDatabase.Instance.GetCard(CardId.BAG_OF_TRICKS);

        JObject expected1 = new JObject
        {
            {"name", "Hlaalu Kinsman"},
            {"deck", "HLAALU"},
            {"cost", 10},
            {"type", "AGENT"},
            {"HP", 1},
            {"taunt", false},
            {"UniqueId", card1.UniqueId.Value },
            {"effects", new JArray("ACQUIRE_TAVERN 9", "REPLACE_TAVERN 1", "", "") }
        };
        Assert.Equal(card1.SerializeObject(), expected1);

        JObject expected2 = new JObject
        {
            {"name", "Way of the Sword"},
            {"deck", "ANSEI"},
            {"cost", 0},
            {"type", "STARTER"},
            {"HP", -1},
            {"taunt", false},
            {"UniqueId", card2.UniqueId.Value },
            {"effects", new JArray("GAIN_POWER 1 OR GAIN_COIN 1", "", "", "") }
        };
        Assert.Equal(card2.SerializeObject(), expected2);

        JObject expected3 = new JObject
        {
            {"name", "Grand Larceny"},
            {"deck", "RAJHIN"},
            {"cost", 5},
            {"type", "ACTION"},
            {"HP", -1},
            {"taunt", false},
            {"UniqueId", card3.UniqueId.Value },
            {"effects", new JArray("GAIN_COIN 4", "KNOCKOUT 1 AND OPP_LOSE_PRESTIGE 1", "", "") }
        };
        Assert.Equal(card3.SerializeObject(), expected3);

        JObject expected4 = new JObject
        {
            {"name", "Bag of Tricks"},
            {"deck", "RAJHIN"},
            {"cost", 7},
            {"type", "CONTRACT_ACTION"},
            {"HP", -1},
            {"taunt", false},
            {"UniqueId", card4.UniqueId.Value },
            {"effects", new JArray("OPP_DISCARD 1", "DRAW 1", "OPP_DISCARD 1", "") }
        };
        Assert.Equal(card4.SerializeObject(), expected4);
    }

    [Fact]
    void SerializePlayer()
    {
        var card1 = GlobalCardDatabase.Instance.GetCard(CardId.HLAALU_KINSMAN);
        var card2 = GlobalCardDatabase.Instance.GetCard(CardId.WAY_OF_THE_SWORD);
        var card3 = GlobalCardDatabase.Instance.GetCard(CardId.GRAND_LARCENY);
        var card4 = GlobalCardDatabase.Instance.GetCard(CardId.BAG_OF_TRICKS);

        var player = new FairSerializedPlayer(new SerializedPlayer(
            PlayerEnum.PLAYER1, new List<UniqueCard> { card1, card2, card3}, new List<UniqueCard>(), new List<UniqueCard> { card4 }, 
            new List<UniqueCard>(), new List<SerializedAgent>(), 4, 1, 20, 16
        ));

        JObject expected = new JObject
        {
            {"Player", "PLAYER1"},
            {"Hand", new JArray(card1.SerializeObject(), card2.SerializeObject(), card3.SerializeObject())},
            {"Cooldown", new JArray(card4.SerializeObject())},
            {"Played", new JArray()},
            {"KnownPile", new JArray()},
            {"Agents", new JArray()},
            {"Power", 4},
            {"PatronCalls", 1},
            {"Coins", 20},
            {"Prestige", 16},
            {"DrawPile", new JArray()}
        };
        Assert.Equal(player.SerializeObject(), expected);
    }
}