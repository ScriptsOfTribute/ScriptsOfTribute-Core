using TalesOfTribute;

namespace SimpleBots;

public static class Extensions
{
    public static int RandomK(int lowerBound, int upperBoaund, SeededRandom rng){
        return (rng.Next() % (upperBoaund + 1 - lowerBound)) + lowerBound;
    }
    public static T PickRandom<T>(this List<T> source, SeededRandom rng)
    {
        return source[rng.Next() % source.Count];
    }
}
