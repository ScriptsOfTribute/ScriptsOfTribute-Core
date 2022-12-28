using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Serializers;

namespace TalesOfTribute;

public interface ISimpleResult
{
}

public abstract class PlayResult
{
}

public class Success : PlayResult, ISimpleResult
{
}

public enum ChoiceFollowUp
{
    ENACT_CHOSEN_EFFECT,
    REPLACE_CARDS_IN_TAVERN,
    DESTROY_CARDS,
    DISCARD_CARDS,
    REFRESH_CARDS,
    TOSS_CARDS,
    KNOCKOUT_AGENTS,
    ACQUIRE_CARDS,
    COMPLETE_HLAALU,
    COMPLETE_PELLIN,
    COMPLETE_PSIJIC,
    COMPLETE_TREASURY,
}

public class Choice : PlayResult
{
    public enum DataType
    {
        EFFECT,
        CARD,
    }

    public List<Card> PossibleCards
    {
        get
        {
            if (Type != DataType.CARD)
            {
                throw new Exception("This choice does not contain Cards.");
            }

            return _possibleCards!;
        }
    }

    public List<Effect> PossibleEffects
    {
        get
        {
            if (Type != DataType.EFFECT)
            {
                throw new Exception("This choice does not contain Effects.");
            }

            return _possibleEffects!;
        }
    }

    public int MaxChoiceAmount { get; } = 1;
    public int MinChoiceAmount { get; } = 0;
    public ChoiceContext? Context { get; }
    public readonly DataType Type;
    public readonly ChoiceFollowUp FollowUp;
    private readonly List<Card>? _possibleCards;
    private readonly List<Effect>? _possibleEffects;

    public Choice(List<Effect> possibleChoices, ChoiceFollowUp followUp, ChoiceContext? context)
    {
        _possibleEffects = possibleChoices;
        Type = DataType.EFFECT;
        FollowUp = followUp;
        Context = context;
    }

    public Choice(List<Effect> possibleChoices, ChoiceFollowUp followUp, ChoiceContext? context, int maxChoiceAmount, int minChoiceAmount = 0) : this(possibleChoices, followUp, context)
    {
        if (minChoiceAmount > possibleChoices.Count)
        {
            throw new Exception("Invalid choice amount specified!");
        }

        MaxChoiceAmount = maxChoiceAmount;
        MinChoiceAmount = minChoiceAmount;
    }
    
    public Choice(List<Card> possibleChoices, ChoiceFollowUp followUp, ChoiceContext? context)
    {
        _possibleCards = possibleChoices;
        Type = DataType.CARD;
        FollowUp = followUp;
        Context = context;
    }

    public Choice(List<Card> possibleChoices, ChoiceFollowUp followUp, ChoiceContext? context, int maxChoiceAmount, int minChoiceAmount = 0) : this(possibleChoices, followUp, context)
    {
        if (minChoiceAmount > possibleChoices.Count)
        {
            throw new Exception("Invalid choice amount specified!");
        }

        MaxChoiceAmount = maxChoiceAmount;
        MinChoiceAmount = minChoiceAmount;
    }

    public SerializedChoice Serialize()
    {
        return new SerializedChoice(MaxChoiceAmount, MinChoiceAmount, Context, _possibleCards, _possibleEffects, FollowUp, Type);
    }
}

// Failure should only be returned on user input error.
// If error is not a consequence of a decision, an exception should be thrown.
public class Failure : PlayResult, ISimpleResult
{
    public string Reason { get; }

    public Failure(string reason)
    {
        Reason = reason;
    }
}
