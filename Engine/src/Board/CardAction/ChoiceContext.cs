using TalesOfTribute.Board.Cards;

namespace TalesOfTribute;

public enum ChoiceType
{
    CARD_EFFECT,
    EFFECT_CHOICE,
    PATRON_ACTIVATION,
}

public class ChoiceContext
{
    // In case of PATRON_ACTIVATION, PatronSource is not null
    // In case of EFFECT_CHOICE, CardSource and Combo are not null
    // In case of CARD_EFFECT, CardSource, Combo and Effect are not null
    public readonly PatronId? PatronSource;
    public readonly UniqueCard? CardSource;
    public readonly ChoiceType ChoiceType;
    public readonly int Combo = 1;

    public UniqueEffect? Effect
    {
        get
        {
            if (ChoiceType != ChoiceType.CARD_EFFECT)
            {
                return null;
            }

            return CardSource!.Effects[Combo] as UniqueEffect;
        }
    }

    public ChoiceContext(PatronId patron)
    {
        PatronSource = patron;
        ChoiceType = ChoiceType.PATRON_ACTIVATION;
    }

    public ChoiceContext(UniqueCard card, ChoiceType type, int combo)
    {
        CardSource = card;
        ChoiceType = type;
        Combo = combo;
    }
}
