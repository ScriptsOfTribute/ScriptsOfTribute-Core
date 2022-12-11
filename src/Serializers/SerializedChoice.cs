namespace TalesOfTribute.Serializers;

public class SerializedChoice<T>
{
    public List<T> PossibleChoices { get; }
    public int MaxChoices { get; }
    public int MinChoices { get; }
    public ChoiceContext Context { get; }

    private SerializedChoice(Choice<T> choice)
    {
        PossibleChoices = new List<T>(choice.PossibleChoices);
        MaxChoices = choice.MaxChoiceAmount;
        MinChoices = choice.MinChoiceAmount;
        Context = choice.Context;
    }


    public static SerializedChoice<T> FromChoice(Choice<T> choice)
    {
        return new SerializedChoice<T>(choice);
    }
}
