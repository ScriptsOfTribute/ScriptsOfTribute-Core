namespace TalesOfTribute.Serializers;

public class SerializedChoice<T>
{
    public List<T> PossibleChoices { get; }
    public int MaxChoices { get; }
    public int MinChoices { get; }
    
    private SerializedChoice(Choice<T> choice)
    {
        PossibleChoices = new List<T>(choice.PossibleChoices);
        MaxChoices = choice.MaxChoiceAmount;
        MinChoices = choice.MinChoiceAmount;
    }


    public static SerializedChoice<T> FromChoice(Choice<T> choice)
    {
        return new SerializedChoice<T>(choice);
    }
}
