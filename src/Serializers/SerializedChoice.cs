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
    public ChoiceContext Context { get; }
}

public class SerializedChoice<T> : BaseSerializedChoice
{
    public List<T> PossibleChoices { get; }

    protected SerializedChoice(Choice<T> choice) : base(choice.MaxChoiceAmount, choice.MinChoiceAmount, choice.Context)
    {
        PossibleChoices = new List<T>(choice.PossibleChoices);
    }

    public static SerializedChoice<T> FromChoice(Choice<T> choice)
    {
        return new SerializedChoice<T>(choice);
    }
}

public class SerializedCardChoice : SerializedChoice<Card>
{
    protected SerializedCardChoice(Choice<Card> choice) : base(choice)
    {
    }
}

public class SerializedEffectChoice : SerializedChoice<EffectType>
{
    protected SerializedEffectChoice(Choice<EffectType> choice) : base(choice)
    {
    }
}
