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
    public readonly Activable activator;
    public readonly ChoiceType choiceType;

    public ChoiceContext(Patron patron)
    {
        activator = patron;
        choiceType = ChoiceType.PATRON_ACTIVATION;
    }

    public ChoiceContext(UniqueId cardID, ChoiceType type)
    {
        activator = GlobalCardDatabase.Instance.GetExistingCard(cardID);
        choiceType = type;
    }
}
