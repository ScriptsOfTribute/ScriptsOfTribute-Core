using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute;

public interface IPlayer
{
    PlayerEnum ID { get; set; }
    int CoinsAmount { get; set; }
    int PrestigeAmount { get; set; }
    int PowerAmount { get; set; }
    public uint PatronCalls { get; set; }
    public int KnownUpcomingDrawsAmount { get; }
    List<UniqueCard> Hand { get; set; }
    List<UniqueCard> DrawPile { get; set; }
    List<UniqueCard> Played { get; set; }
    List<Agent> Agents { get; set; }
    List<UniqueCard> AgentCards { get; }
    List<UniqueCard> CooldownPile { get; set; }
    void PlayCard(UniqueCard cardId);
    int HealAgent(UniqueCard card, int amount);
    void Refresh(UniqueCard cardId);
    void Draw(int amount);
    List<UniqueCard> PrepareToss(int amount);
    void EndTurn();
    void Toss(UniqueCard cardId);
    void Discard(UniqueCard card);
    void KnockOut(UniqueCard cardId, ITavern tavern);
    void KnockOutAll(ITavern tavern);
    void AddToCooldownPile(UniqueCard card);
    void Destroy(UniqueCard cardId);
    string ToString();
    List<UniqueCard> GetAllPlayersCards();
    void ActivateAgent(UniqueCard card);
    int AttackAgent(UniqueCard agent, IPlayer enemy, ITavern tavern);
}
