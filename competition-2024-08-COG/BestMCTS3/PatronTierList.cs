using ScriptsOfTribute;

namespace Bots;

public record struct PatronTier(int favoured, int neutral, int unfavoured);

public static class PatronTierList
{
    static readonly Dictionary<PatronId, PatronTier[]> patronTierDict = new Dictionary<PatronId, PatronTier[]> {
        { PatronId.ANSEI,         new PatronTier[] { new(1, 0, -1), new(1, 0, -1), new(1, 0, -1) } },
        { PatronId.DUKE_OF_CROWS, new PatronTier[] { new(-1, 0, 1), new(-1, 0, 1), new(-1, 0, 1) } },
        { PatronId.HLAALU,        new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.PELIN,         new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.RAJHIN,        new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.RED_EAGLE,     new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.ORGNUM,        new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
        { PatronId.TREASURY,      new PatronTier[] { new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
    };

    public static PatronTier GetPatronTier(PatronId patron, GamePhase gamePhase) => patronTierDict[patron][(int)gamePhase];
}