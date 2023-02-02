using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute.Serializers;

public class SerializedPlayer
{
    public readonly PlayerEnum PlayerID;
    public readonly List<UniqueCard> Hand;
    public readonly List<UniqueCard> DrawPile;
    public readonly List<UniqueCard> CooldownPile;
    public readonly List<UniqueCard> Played;
    public readonly List<UniqueCard> KnownUpcomingDraws;
    public readonly List<SerializedAgent> Agents;
    public readonly int Power;
    public readonly uint PatronCalls;
    public readonly int Coins;
    public readonly int Prestige;

    public SerializedPlayer(PlayerEnum playerId, List<UniqueCard> hand, List<UniqueCard> drawPile, List<UniqueCard> cooldownPile, List<UniqueCard> played, List<SerializedAgent> agents, int power, uint patronCalls, int coins, int prestige)
    {
        PlayerID = playerId;
        Hand = hand;
        DrawPile = drawPile;
        CooldownPile = cooldownPile;
        Played = played;
        Agents = agents;
        Power = power;
        PatronCalls = patronCalls;
        Coins = coins;
        Prestige = prestige;
        KnownUpcomingDraws = new List<UniqueCard>();
    }

    public SerializedPlayer(IPlayer player)
    {
        PlayerID = player.ID;
        Hand = player.Hand;
        DrawPile = player.DrawPile;
        CooldownPile = player.CooldownPile;
        Played = player.Played;
        Agents = player.Agents.Select(agent => new SerializedAgent(agent)).ToList();
        Power = player.PowerAmount;
        PatronCalls = player.PatronCalls;
        Coins = player.CoinsAmount;
        Prestige = player.PrestigeAmount;
        KnownUpcomingDraws = DrawPile.Take(player.KnownUpcomingDrawsAmount).ToList();
    }
}
