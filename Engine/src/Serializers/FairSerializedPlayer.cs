using Newtonsoft.Json.Linq;
using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute.Serializers;

public class FairSerializedEnemyPlayer
{
    public PlayerEnum PlayerID => _player.PlayerID;
    public List<SerializedAgent> Agents => _player.Agents;
    public int Power => _player.Power;
    public int Coins => _player.Coins;
    public int Prestige => _player.Prestige;

    private List<UniqueCard>? _handAndDraw;
    public List<UniqueCard> HandAndDraw => _handAndDraw ??= _player.DrawPile.Concat(_player.Hand).OrderBy(c => c.CommonId).ToList();

    public List<UniqueCard> Played => _player.Played;
    public List<UniqueCard> CooldownPile => _player.CooldownPile;

    private readonly SerializedPlayer _player;

    public FairSerializedEnemyPlayer(SerializedPlayer player)
    {
        _player = player;
    }

    public JObject SerializeObject()
    {
        JObject obj = new JObject
        {
            {"Player", PlayerID.ToString()},
            {"Cooldown", new JArray(_player.CooldownPile.Select(card => card.SerializeObject()).ToList())},
            {"Played", new JArray(_player.Played.Select(card => card.SerializeObject()).ToList())},
            {"HandAndDraw", new JArray(HandAndDraw.Select(card => card.SerializeObject()).ToList())},
            {"Agents", new JArray(_player.Agents.Select(agent => agent.SerializeObject()).ToList())},
            {"Power", _player.Power},
            {"Coins", _player.Coins},
            {"Prestige", _player.Prestige},
        };

        return obj;
    }
}

public class FairSerializedPlayer
{
    public PlayerEnum PlayerID => _player.PlayerID;
    public List<UniqueCard> Hand => _player.Hand;
    // Cooldown Pile can be returned as-is, because it gets reordered anyway.
    public List<UniqueCard> CooldownPile => _player.CooldownPile;
    public List<UniqueCard> Played => _player.Played;
    public List<UniqueCard> KnownUpcomingDraws => _player.KnownUpcomingDraws;
    public List<SerializedAgent> Agents => _player.Agents;
    public int Power => _player.Power;
    public uint PatronCalls => _player.PatronCalls;
    public int Coins => _player.Coins;
    public int Prestige => _player.Prestige;

    private List<UniqueCard>? _drawPile;
    public List<UniqueCard> DrawPile => _drawPile ??= _player.DrawPile.OrderBy(c => c.CommonId).ToList();

    private readonly SerializedPlayer _player;

    public FairSerializedPlayer(SerializedPlayer player)
    {
        _player = player;
    }

    public JObject SerializeObject()
    {
        JObject obj = new JObject
        {
            {"Player", PlayerID.ToString()},
            {"Hand", new JArray(_player.Hand.Select(card => card.SerializeObject()).ToList())},
            {"Cooldown", new JArray(_player.CooldownPile.Select(card => card.SerializeObject()).ToList())},
            {"Played", new JArray(_player.Played.Select(card => card.SerializeObject()).ToList())},
            {"KnownPile", new JArray(_player.KnownUpcomingDraws.Select(card => card.SerializeObject()).ToList())},
            {"Agents", new JArray(_player.Agents.Select(agent => agent.SerializeObject()).ToList())},
            {"Power", _player.Power},
            {"PatronCalls", _player.PatronCalls},
            {"Coins", _player.Coins},
            {"Prestige", _player.Prestige},
            {"DrawPile",  new JArray(_player.DrawPile.Select(card => card.SerializeObject()).ToList())}
        };

        return obj;
    }
}
