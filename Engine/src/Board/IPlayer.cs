namespace TalesOfTribute;

public interface IPlayer
{
    PlayerEnum ID { get; set; }
    int CoinsAmount { get; set; }
    int PrestigeAmount { get; set; }
    int PowerAmount { get; set; }
    public uint PatronCalls { get; set; }
    List<Card> Hand { get; set; }
    List<Card> DrawPile { get; set; }
    List<Card> Played { get; set; }
    List<Agent> Agents { get; set; }
    List<Card> AgentCards { get; }
    List<Card> CooldownPile { get; set; }
    void PlayCard(Card cardId);
    int HealAgent(UniqueId uniqueId, int amount);
    void Refresh(Card cardId);
    void Draw();
    void EndTurn();
    void Toss(Card cardId);
    void Discard(Card card);
    void KnockOut(Card cardId);
    void AddToCooldownPile(Card card);
    void Destroy(Card cardId);
    string ToString();
    List<Card> GetAllPlayersCards();
    void ActivateAgent(Card card);
    int AttackAgent(Card agent, IPlayer enemy, ITavern tavern);
    Card GetCardByUniqueId(int uniqueId);
}
