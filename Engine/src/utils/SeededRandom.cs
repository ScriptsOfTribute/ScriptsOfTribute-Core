namespace ScriptsOfTribute;

public class SeededRandom
{
    public ulong InitialSeed { get; private set; }
    public ulong CurrentSeed { get; private set; }

    public SeededRandom(ulong initialSeed)
    {
        InitialSeed = initialSeed;
        CurrentSeed = initialSeed;
    }

    public SeededRandom(ulong initialSeed, ulong currentSeed)
    {
        InitialSeed = initialSeed;
        CurrentSeed = currentSeed;
    }

    public SeededRandom() : this((ulong)Environment.TickCount)
    {
    }

    public int Next()
    {
        CurrentSeed = (0x5DEECE66DUL * CurrentSeed + 0xBUL) & ((1UL << 48) - 1);
        return (int)(((CurrentSeed >> 16) * (int.MaxValue - 1)) >> 32);        
    }
}
