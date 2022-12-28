namespace TalesOfTribute.Serializers;

public class ComboState
{
    // TODO: Make this readonly.
    public List<BaseEffect>[] All { get; } = new List<BaseEffect>[Combo.MAX_COMBO];
    public readonly int CurrentCombo;

    public ComboState(List<BaseEffect>[] combos, int currentCombo)
    {
        All = combos;
        CurrentCombo = currentCombo;
    }
}

public class ComboStates
{
    // TODO: Make this Immutable/ReadOnly
    public Dictionary<PatronId, ComboState> All = new();
    
    public ComboStates(Dictionary<PatronId, ComboState> states)
    {
        All = states;
    }

    ComboState GetFor(PatronId patron)
    {
        if (!All.TryGetValue(patron, out var result))
        {
            throw new Exception($"Patron {patron.ToString()} is not in the game.");
        }

        return result;
    }
}
