﻿namespace ScriptsOfTribute.Serializers;

public class PatronStates
{
    // TODO: Make this Immutable/ReadOnly
    public readonly Dictionary<PatronId, PlayerEnum> All = new();

    public PatronStates(List<Patron> patrons)
    {
        patrons.ForEach(patron => All.Add(patron.PatronID, patron.FavoredPlayer));
    }

    public PlayerEnum GetFor(PatronId patron)
    {
        if (!All.TryGetValue(patron, out var result))
        {
            throw new EngineException($"Patron {patron.ToString()} is not in the game.");
        }

        return result;
    }
}
