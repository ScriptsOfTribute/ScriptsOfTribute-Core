namespace TalesOfTribute.Serializers;

public abstract class BaseSerializedChoice
{
    protected BaseSerializedChoice(int maxChoices, int minChoices, ChoiceContext context)
    {
        MaxChoices = maxChoices;
        MinChoices = minChoices;
        Context = context;
    }

    public int MaxChoices { get; }
    public int MinChoices { get; }
    public ChoiceContext? Context { get; }
    
    public static BaseSerializedChoice? FromBaseChoice(BaseChoice? c)
    {
        return c switch
        {
            Choice<Card> ch => ch.Serialize(),
            Choice<EffectType> ch => ch.Serialize(),
            _ => null,
        };
    }

    public static BaseChoice? ToChoice(BaseSerializedChoice? c)
    {
        return c switch
        {
            SerializedChoice<Card> ch => ch.ToChoice(),
            SerializedChoice<EffectType> ch => ch.ToChoice(),
            _ => null,
        };
    }
}

public class SerializedChoice<T> : BaseSerializedChoice
{
    public List<T> PossibleChoices { get; }
    private Choice<T>.ChoiceCallback _callback;

    public SerializedChoice(int maxChoices, int minChoices, ChoiceContext context, List<T> possibleChoices, Choice<T>.ChoiceCallback callback) : base(maxChoices, minChoices, context)
    {
        PossibleChoices = possibleChoices.ToList();
        _callback = callback;
    }

    public Choice<T> ToChoice()
    {
        return new Choice<T>(PossibleChoices.ToList(), (Choice<T>.ChoiceCallback)_callback.Clone(), Context, MaxChoices,
            MinChoices);
    }
}

public class SerializedCardChoice : SerializedChoice<Card>
{
    protected SerializedCardChoice(int maxChoices, int minChoices, ChoiceContext context, List<Card> possibleChoices, Choice<Card>.ChoiceCallback callback)
        : base(maxChoices, minChoices, context, possibleChoices, callback)
    {
    }
}

public class SerializedEffectChoice : SerializedChoice<EffectType>
{
    protected SerializedEffectChoice(int maxChoices, int minChoices, ChoiceContext context, List<EffectType> possibleChoices, Choice<EffectType>.ChoiceCallback callback)
        : base(maxChoices, minChoices, context, possibleChoices, callback)
    {
    }
}
