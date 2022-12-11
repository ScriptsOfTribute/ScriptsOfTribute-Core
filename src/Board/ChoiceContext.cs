namespace TalesOfTribute;

public enum ChoiceType
{
    REPLACE_TAVERN,
    ACQUIRE_TAVERN,
    DESTROY_CARD,
    OPP_DISCARD,
    RETURN_TOP,
    TOSS,
    KNOCKOUT,
    OR,
    PATRON_ACTIVATION,
}

public abstract class Activable
{

}

public class ChoiceContext
{
    public readonly Activable Activator;
    public readonly ChoiceType ChoiceType;

    public ChoiceContext(Patron patron)
    {
        Activator = patron;
        ChoiceType = ChoiceType.PATRON_ACTIVATION;
    }

    public ChoiceContext(UniqueId cardId, ChoiceType type)
    {
        Activator = GlobalCardDatabase.Instance.GetExistingCard(cardId);
        ChoiceType = type;
    }
}
