using TalesOfTribute.Board.Cards;

namespace TalesOfTribute;

public interface IPlayer
{
    PlayerEnum ID { get; set; }
    int CoinsAmount { get; set; }
    int PrestigeAmount { get; set; }
    int PowerAmount { get; set; }
    public uint PatronCalls { get; set; }
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
    void PrepareToss(int amount);
    void EndTurn();
    void Toss(UniqueCard cardId);
    void Discard(UniqueCard card);
    void KnockOut(UniqueCard cardId);
    void AddToCooldownPile(UniqueCard card);
    void Destroy(UniqueCard cardId);
    string ToString();
    List<UniqueCard> GetAllPlayersCards();
    void ActivateAgent(UniqueCard card);
    int AttackAgent(UniqueCard agent, IPlayer enemy, ITavern tavern);
    UniqueCard GetCardByUniqueId(int uniqueId);
}
