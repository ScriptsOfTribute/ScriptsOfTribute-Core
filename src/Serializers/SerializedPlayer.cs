namespace TalesOfTribute.Serializers;

public class SerializedPlayer
{
    public readonly PlayerEnum PlayerID;
    public readonly List<Card> Hand;
    public readonly List<Card> DrawPile;
    public readonly List<Card> CooldownPile;
    public readonly List<Card> Played;
    public readonly List<SerializedAgent> Agents;
    public readonly int Power;
    public readonly uint PatronCalls;
    public readonly int Coins;
    public readonly int Prestige;


    public SerializedPlayer(IPlayer player)
    {
        PlayerID = player.ID;
        Hand = player.Hand.ToList();
        DrawPile = player.DrawPile.ToList();
        CooldownPile = player.CooldownPile.ToList();
        Played = player.Played.ToList();
        Agents = player.Agents.Select(agent => new SerializedAgent(agent)).ToList();
        Power = player.PowerAmount;
        PatronCalls = player.PatronCalls;
        Coins = player.CoinsAmount;
        Prestige = player.PrestigeAmount;
    }
}
