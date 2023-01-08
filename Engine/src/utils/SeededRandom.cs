namespace TalesOfTribute;

public class SeededRandom
{
    public ulong Seed { get; private set; }

    public SeededRandom(ulong seed)
    {
        Seed = seed;
    }

    public SeededRandom() : this((ulong)Environment.TickCount)
    {
    }

    public int Next()
    {
        Seed = (0x5DEECE66DUL * Seed + 0xBUL) & ((1UL << 48) - 1);
        return (int)(((Seed >> 16) * (int.MaxValue - 1)) >> 32);        
    }
}
