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

public abstract class BaseChoice : PlayResult
{
    protected BaseChoice(ChoiceFollowUp choiceFollowUp)
    {
        ChoiceFollowUp = choiceFollowUp;
    }

    public ChoiceFollowUp ChoiceFollowUp { get; }

    public BaseSerializedChoice Serialize()
    {
        return this switch
        {
            Choice<Card> c => new SerializedCardChoice(c.MaxChoiceAmount, c.MinChoiceAmount, c.Context, c.PossibleChoices, c.ChoiceFollowUp),
            Choice<Effect> c => new SerializedEffectChoice(c.MaxChoiceAmount, c.MinChoiceAmount, c.Context, c.PossibleChoices, c.ChoiceFollowUp),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
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

public interface IChoosable
{
    
}

public class Choice<T> : BaseChoice where T : IChoosable
{
    public List<T> PossibleChoices { get; }
    public int MaxChoiceAmount { get; } = 1;
    public int MinChoiceAmount { get; } = 0;
    public ChoiceContext? Context { get; }

    public Choice(List<T> possibleChoices, ChoiceFollowUp followUp, ChoiceContext? context) : base(followUp)
    {
        PossibleChoices = possibleChoices;
        Context = context;
    }

    public Choice(List<T> possibleChoices, ChoiceFollowUp followUp, ChoiceContext? context, int maxChoiceAmount, int minChoiceAmount = 0) : this(possibleChoices, followUp, context)
    {
        if (minChoiceAmount > possibleChoices.Count)
        {
            throw new Exception("Invalid choice amount specified!");
        }

        MaxChoiceAmount = maxChoiceAmount;
        MinChoiceAmount = minChoiceAmount;
    }

    public SerializedChoice<T> Serialize()
    {
        return new SerializedChoice<T>(MaxChoiceAmount, MinChoiceAmount, Context, PossibleChoices, ChoiceFollowUp);
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
