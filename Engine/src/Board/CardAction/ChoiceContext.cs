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

public interface IChoiceSource
{

}

public class ChoiceContext
{
    public readonly IChoiceSource Source;
    public readonly ChoiceType ChoiceType;

    public ChoiceContext(Patron patron)
    {
        Source = patron;
        ChoiceType = ChoiceType.PATRON_ACTIVATION;
    }

    public ChoiceContext(UniqueId cardId, ChoiceType type)
    {
        Source = GlobalCardDatabase.Instance.GetExistingCard(cardId);
        ChoiceType = type;
    }
}
