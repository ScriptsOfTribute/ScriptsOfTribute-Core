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
    public readonly Card? CardSource;
    public readonly ChoiceType ChoiceType;
    public readonly int Combo = 1;

    public Effect? Effect
    {
        get
        {
            if (ChoiceType != ChoiceType.CARD_EFFECT)
            {
                return null;
            }

            return CardSource!.Effects[Combo] as Effect;
        }
    }

    public ChoiceContext(PatronId patron)
    {
        PatronSource = patron;
        ChoiceType = ChoiceType.PATRON_ACTIVATION;
    }

    public ChoiceContext(UniqueId cardId, ChoiceType type, int combo)
    {
        CardSource = GlobalCardDatabase.Instance.GetExistingCard(cardId);
        ChoiceType = type;
        Combo = combo;
    }
}
