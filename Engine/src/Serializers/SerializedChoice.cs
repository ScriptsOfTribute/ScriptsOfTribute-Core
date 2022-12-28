namespace TalesOfTribute.Serializers;

public abstract class BaseSerializedChoice
{
    protected BaseSerializedChoice(int maxChoices, int minChoices, ChoiceContext? context, ChoiceFollowUp choiceFollowUp)
    {
        MaxChoices = maxChoices;
        MinChoices = minChoices;
        Context = context;
        ChoiceFollowUp = choiceFollowUp;
    }

    public int MaxChoices { get; }
    public int MinChoices { get; }
    public ChoiceContext? Context { get; }   
    public ChoiceFollowUp ChoiceFollowUp { get; }

    public BaseChoice ToChoice()
    {
        return this switch
        {
            SerializedCardChoice c =>
                new Choice<Card>(c.PossibleChoices, ChoiceFollowUp, Context, MaxChoices, MinChoices),
            SerializedEffectChoice c =>
                new Choice<Effect>(c.PossibleChoices, ChoiceFollowUp, Context, MaxChoices, MinChoices),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public class SerializedChoice<T> : BaseSerializedChoice
{
    public List<T> PossibleChoices { get; }

    public SerializedChoice(int maxChoices, int minChoices, ChoiceContext context, List<T> possibleChoices, ChoiceFollowUp choiceFollowUp) : base(maxChoices, minChoices, context, choiceFollowUp)
    {
        PossibleChoices = possibleChoices;
    }
}

public class SerializedCardChoice : SerializedChoice<Card>
{
    public SerializedCardChoice(int maxChoices, int minChoices, ChoiceContext context, List<Card> possibleChoices, ChoiceFollowUp choiceFollowUp)
        : base(maxChoices, minChoices, context, possibleChoices, choiceFollowUp)
    {
    }
}

public class SerializedEffectChoice : SerializedChoice<Effect>
{
    public SerializedEffectChoice(int maxChoices, int minChoices, ChoiceContext context, List<Effect> possibleChoices, ChoiceFollowUp choiceFollowUp)
        : base(maxChoices, minChoices, context, possibleChoices, choiceFollowUp)
    {
    }
}
