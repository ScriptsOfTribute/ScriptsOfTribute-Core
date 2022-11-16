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
    List<Card> Agents { get; set; }
    List<Card> CooldownPile { get; set; }
    ExecutionChain PlayCard(CardId cardId, IPlayer other, ITavern tavern);
    void HandleAcquireDuringExecutionChain(Card card, IPlayer other, ITavern tavern);
    void HealAgent(Guid guid, int amount);
    void Refresh(CardId cardId);
    void Draw();
    void EndTurn();
    ExecutionChain AcquireCard(Card card, IPlayer enemy, ITavern tavern, bool replacePendingExecutionChain=true);
    void Toss(CardId cardId);
    void KnockOut(CardId cardId);
    void AddToCooldownPile(Card card);
    void DestroyInHand(CardId cardId);
    void DestroyAgent(CardId cardId);
    string ToString();
    List<Card> GetAllPlayersCards();
}
