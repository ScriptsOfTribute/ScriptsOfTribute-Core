using System.Runtime.Serialization.Formatters.Binary;

namespace TalesOfTribute;

public class MyRandom : Random
{
    private int _seed;
    private int _invocationCount = 0;

    public MyRandom(int seed) : base(seed)
    {
        _seed = seed;
    }

    public override int Next()
    {
        _invocationCount++;
        return base.Next();
    }

    public MyRandom DeepClone()
    {
        var res = new MyRandom(_seed);
        for (var i = 0; i < _invocationCount; i++)
        {
            res.Next();
        }

        return res;
    }
}

public class SeededRandom
{
    private readonly MyRandom _internalRandom;
    public readonly int Seed;

    public SeededRandom(int seed)
    {
        Seed = seed;
        _internalRandom = new MyRandom(Seed);
    }

    public SeededRandom() : this(Environment.TickCount)
    {
    }

    public int Next() => _internalRandom.Next();

    private SeededRandom(MyRandom random, int seed)
    {
        _internalRandom = random;
        Seed = seed;
    }

    public SeededRandom Detach()
    {
        return new SeededRandom(_internalRandom.DeepClone(), Seed);
    }
}
