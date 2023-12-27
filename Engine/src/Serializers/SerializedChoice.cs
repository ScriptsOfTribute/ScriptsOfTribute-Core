using Newtonsoft.Json.Linq;
using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute.Serializers;

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

    public JObject SerializeObject()
    {
        JObject json = new JObject(
            new JProperty("MaxChoices", MaxChoices),
            new JProperty("MinChoices", MinChoices),
            new JProperty("Context", Context.ToString()),
            new JProperty("ChoiceFollowUp", ChoiceFollowUp.ToString()),
            new JProperty("Type", Type.ToString())
        );

        if (Type == Choice.DataType.CARD)
        {
            json.Add("PossibleCards", new JArray(_possibleCards.Select(card => card.SerializeObject()).ToList()));
        }
        else if (Type == Choice.DataType.EFFECT)
        {
            json.Add("PossibleEffects", new JArray(_possibleEffects.Select(effect => effect.ToSimpleString()).ToList()));
        }

        return json;
    }
}
