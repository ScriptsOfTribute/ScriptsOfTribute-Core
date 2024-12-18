using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute.Serializers;

public class ComboState
{
    // TODO: Make this readonly.
    public List<UniqueSimpleEffect>[] All { get; } = new List<UniqueSimpleEffect>[Combo.MAX_COMBO];
    public readonly int CurrentCombo;

    public ComboState(List<UniqueSimpleEffect>[] combos, int currentCombo)
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
            throw new EngineException($"Patron {patron.ToString()} is not in the game.");
        }

        return result;
    }
}
