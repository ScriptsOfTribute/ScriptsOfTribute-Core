﻿namespace SimpleBots;

public static class Extensions
{
    private static readonly Random Rnd = new();

    public static T PickRandom<T>(this List<T> source)
    {
        return source[Rnd.Next(source.Count)];
    }
}
