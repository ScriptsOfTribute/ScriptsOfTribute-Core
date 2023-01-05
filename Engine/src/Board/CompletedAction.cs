using System.Text;
using TalesOfTribute.Board.Cards;

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
    ADD_WRIT_OF_COIN,
    HEAL_AGENT,
    END_TURN,
}

public class CompletedAction
{
    public CompletedActionType Type;
    public UniqueCard? SourceCard;
    public UniqueCard? TargetCard;
    public PatronId? SourcePatron;
    public readonly int Combo = 1;
    public readonly int Amount = 1;

    public CompletedAction(CompletedActionType type)
    {
        Type = type;
    }

    public CompletedAction(CompletedActionType type, UniqueCard sourceCard, int amount)
    {
        Type = type;
        SourceCard = sourceCard;
        Amount = amount;
    }

    public CompletedAction(CompletedActionType type, UniqueCard? sourceCard, int amount, UniqueCard targetCard)
    {
        Type = type;
        SourceCard = sourceCard;
        Amount = amount;
        TargetCard = targetCard;
    }
    
    public CompletedAction(CompletedActionType type, UniqueCard sourceCard, UniqueCard targetCard)
    {
        Type = type;
        SourceCard = sourceCard;
        TargetCard = targetCard;
    }
    
    public CompletedAction(CompletedActionType type, PatronId sourcePatron, UniqueCard targetCard)
    {
        Type = type;
        SourcePatron = sourcePatron;
        TargetCard = targetCard;
    }
    
    public CompletedAction(CompletedActionType type, PatronId sourcePatron, int amount)
    {
        Type = type;
        SourcePatron = sourcePatron;
        Amount = amount;
    }
    
    public CompletedAction(CompletedActionType type, PatronId sourcePatron, int amount, UniqueCard targetCard)
    {
        Type = type;
        SourcePatron = sourcePatron;
        Amount = amount;
        TargetCard = targetCard;
    }
    
    public CompletedAction(CompletedActionType type, UniqueCard targetCard)
    {
        Type = type;
        TargetCard = targetCard;
    }
    
    public CompletedAction(CompletedActionType type, PatronId sourcePatronId)
    {
        Type = type;
        SourcePatron = sourcePatronId;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        switch (Type)
        {
            case CompletedActionType.BUY_CARD:
                sb.Append($"Buy Card - {TargetCard}");
                break;
            case CompletedActionType.ACQUIRE_CARD:
                sb.Append($"Acquire Card - Source: {SourceCard}{SourcePatron} Target: {TargetCard}");
                break;
            case CompletedActionType.PLAY_CARD:
                sb.Append($"Play Card - {TargetCard}");
                break;
            case CompletedActionType.ACTIVATE_AGENT:
                sb.Append($"Activate Agent - {TargetCard}");
                break;
            case CompletedActionType.ACTIVATE_PATRON:
                sb.Append($"Activate Patron - {SourcePatron}");
                break;
            case CompletedActionType.ATTACK_AGENT:
                sb.Append($"Attack Agent - {TargetCard} for {Amount}");
                break;
            case CompletedActionType.AGENT_DEATH:
                sb.Append($"Agent Death - {TargetCard}");
                break;
            case CompletedActionType.GAIN_COIN:
                sb.Append($"Gain Coin - Amount: {Amount} Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.GAIN_POWER:
                sb.Append($"Gain Power - Amount: {Amount} Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.GAIN_PRESTIGE:
                sb.Append($"Gain Prestige - Amount: {Amount} Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.OPP_LOSE_PRESTIGE:
                sb.Append($"Opp Lose Prestige - Amount: {Amount} Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.REPLACE_TAVERN:
                sb.Append($"Replace Tavern - Source: {SourceCard}{SourcePatron} Target: {TargetCard}");
                break;
            case CompletedActionType.DESTROY_CARD:
                sb.Append($"Destroy Card - Source: {SourceCard}{SourcePatron} Target: {TargetCard}");
                break;
            case CompletedActionType.DRAW:
                sb.Append($"Draw - Amount: {Amount} Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.DISCARD:
                sb.Append($"Discard - Source: {SourceCard}{SourcePatron} Target: {TargetCard}");
                break;
            case CompletedActionType.REFRESH:
                sb.Append($"Refresh - Source: {SourceCard}{SourcePatron} Target: {TargetCard}");
                break;
            case CompletedActionType.TOSS:
                sb.Append($"Toss - Source: {SourceCard}{SourcePatron} Target: {TargetCard}");
                break;
            case CompletedActionType.KNOCKOUT:
                sb.Append($"Knockout - Source: {SourceCard}{SourcePatron} Target: {TargetCard}");
                break;
            case CompletedActionType.ADD_BOARDING_PARTY:
                sb.Append($"Add Boarding Party - Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.ADD_BEWILDERMENT_TO_OPPONENT:
                sb.Append($"Add Bewilderment To Opponent - Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.HEAL_AGENT:
                sb.Append($"Heal Agent: Amount: {Amount} Agent: {TargetCard} Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.END_TURN:
                sb.Append("End Turn");
                break;
            case CompletedActionType.ADD_PATRON_CALLS:
                sb.Append($"Increment patron calls - Amount: {Amount} Source: {SourceCard}{SourcePatron}");
                break;
            case CompletedActionType.ADD_WRIT_OF_COIN:
                sb.Append($"Add Writ Of Coin - Source {SourceCard}{SourcePatron}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return sb.ToString();
    }
}
