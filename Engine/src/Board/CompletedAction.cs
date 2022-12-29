using System.Text;

namespace TalesOfTribute.Board;

public enum CompletedActionType
{
    BUY_CARD,
    ACQUIRE_CARD,
    PLAY_CARD,
    ACTIVATE_AGENT,
    ACTIVATE_PATRON,
    ATTACK_AGENT,
    AGENT_DEATH,
    GAIN_COIN,
    GAIN_POWER,
    GAIN_PRESTIGE,
    OPP_LOSE_PRESTIGE,
    REPLACE_TAVERN,
    DESTROY_CARD,
    DRAW,
    DISCARD,
    REFRESH,
    TOSS,
    KNOCKOUT,
    ADD_PATRON_CALLS,
    ADD_BOARDING_PARTY,
    ADD_BEWILDERMENT_TO_OPPONENT,
    HEAL_AGENT,
    END_TURN,
}

public class CompletedAction
{
    public CompletedActionType Type;
    public Card? Card;
    public PatronId? Patron;
    public readonly int Combo = 1;
    public readonly int Amount = 1;

    public CompletedAction(CompletedActionType type)
    {
        Type = type;
    }

    public CompletedAction(CompletedActionType type, Card card, int amount)
    {
        Type = type;
        Card = card;
        Amount = amount;
    }
    
    public CompletedAction(CompletedActionType type, PatronId patron, int amount)
    {
        Type = type;
        Patron = patron;
        Amount = amount;
    }
    
    public CompletedAction(CompletedActionType type, Card card)
    {
        Type = type;
        Card = card;
    }
    
    public CompletedAction(CompletedActionType type, PatronId patronId)
    {
        Type = type;
        Patron = patronId;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        switch (Type)
        {
            case CompletedActionType.BUY_CARD:
                sb.Append($"Buy Card: {Card}");
                break;
            case CompletedActionType.ACQUIRE_CARD:
                sb.Append($"Acquire Card: {Card}");
                break;
            case CompletedActionType.PLAY_CARD:
                sb.Append($"Play Card: {Card}");
                break;
            case CompletedActionType.ACTIVATE_AGENT:
                sb.Append($"Activate Agent: {Card}");
                break;
            case CompletedActionType.ACTIVATE_PATRON:
                sb.Append($"Activate Patron: {Patron}");
                break;
            case CompletedActionType.ATTACK_AGENT:
                sb.Append($"Attack Agent: {Card} for {Amount}");
                break;
            case CompletedActionType.AGENT_DEATH:
                sb.Append($"Agent Death: {Card}");
                break;
            case CompletedActionType.GAIN_COIN:
                sb.Append($"Gain Coin - Amount: {Amount} Source: {Card}{Patron}");
                break;
            case CompletedActionType.GAIN_POWER:
                sb.Append($"Gain Power - Amount: {Amount} Source: {Card}{Patron}");
                break;
            case CompletedActionType.GAIN_PRESTIGE:
                sb.Append($"Gain Prestige - Amount: {Amount} Source: {Card}{Patron}");
                break;
            case CompletedActionType.OPP_LOSE_PRESTIGE:
                sb.Append($"Opp Lose Prestige - Amount: {Amount} Source: {Card}{Patron}");
                break;
            case CompletedActionType.REPLACE_TAVERN:
                sb.Append($"Opp Lose Prestige - {Card}");
                break;
            case CompletedActionType.DESTROY_CARD:
                sb.Append($"Destroy Card - {Card}");
                break;
            case CompletedActionType.DRAW:
                sb.Append($"Draw - {Amount}");
                break;
            case CompletedActionType.DISCARD:
                sb.Append($"Discard - {Card}");
                break;
            case CompletedActionType.REFRESH:
                sb.Append($"Refresh - {Card}");
                break;
            case CompletedActionType.TOSS:
                sb.Append($"Toss - {Card}");
                break;
            case CompletedActionType.KNOCKOUT:
                sb.Append($"Knockout - {Card}");
                break;
            case CompletedActionType.ADD_BOARDING_PARTY:
                sb.Append($"Add Boarding Party");
                break;
            case CompletedActionType.ADD_BEWILDERMENT_TO_OPPONENT:
                sb.Append($"Add Bewilderment To Opponent");
                break;
            case CompletedActionType.HEAL_AGENT:
                sb.Append($"Heal Agent: Amount {Amount} Agent {Card}");
                break;
            case CompletedActionType.END_TURN:
                sb.Append("End Turn");
                break;
            case CompletedActionType.ADD_PATRON_CALLS:
                sb.Append($"Increment patron call amount by {Amount}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return sb.ToString();
    }
}
