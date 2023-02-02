namespace ScriptsOfTribute;

public static class Extensions
{
    public static List<List<T>> GetCombinations<T>(this List<T> list, int k)
    {
        var result = new List<List<T>>();

        switch (k)
        {
            case 0:
                return new List<List<T>> { new() };
            case 1:
                return list.Select(i => new List<T> { i }).ToList();
        }

        if (k > list.Count)
        {
            return result;
        }

        if (k == list.Count)
        {
            return new List<List<T>> { list };
        }

        for (var i = 0; i < list.Count - 1; i++)
        {
            var elem = list[i];
            var rest = list.Skip(i + 1).ToList();
            result.AddRange(rest.GetCombinations(k - 1).Select(l => l.Prepend(elem).ToList()));
        }
        
        return result;
    }
}
