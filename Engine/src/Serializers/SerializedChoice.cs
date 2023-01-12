using TalesOfTribute.Board.Cards;

namespace TalesOfTribute.Serializers;

public class SerializedChoice
{
    public List<UniqueCard> PossibleCards
    {
        get
        {
            if (Type != Choice.DataType.CARD)
            {
                throw new EngineException("This choice is not of type CARD, so it doesn't contain cards.");
            }

            return _possibleCards!;
        }
    }

    public List<UniqueEffect> PossibleEffects
    {
        get
        {
            if (Type != Choice.DataType.EFFECT)
            {
                throw new EngineException("This choice is not of type EFFECT, so it doesn't contain effects.");
            }

            return _possibleEffects!;
        }
    }

    public int MaxChoices { get; }
    public int MinChoices { get; }
    public ChoiceContext Context { get; }   
    public ChoiceFollowUp ChoiceFollowUp { get; }
    public readonly Choice.DataType Type;
    private readonly List<UniqueCard>? _possibleCards;
    private readonly List<UniqueEffect>? _possibleEffects;

    public SerializedChoice(int maxChoices, int minChoices, ChoiceContext context, List<UniqueCard>? possibleCards, List<UniqueEffect>? possibleEffects, ChoiceFollowUp choiceFollowUp, Choice.DataType type)
    {
        MaxChoices = maxChoices;
        MinChoices = minChoices;
        Context = context;
        ChoiceFollowUp = choiceFollowUp;
        _possibleCards = possibleCards?.ToList();
        _possibleEffects = possibleEffects?.ToList();
        Type = type;
    }

    public Choice ToChoice()
    {
        return Type switch
        {
            Choice.DataType.EFFECT => new Choice(PossibleEffects, ChoiceFollowUp, Context, MaxChoices, MinChoices),
            Choice.DataType.CARD => new Choice(PossibleCards, ChoiceFollowUp, Context, MaxChoices, MinChoices),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
