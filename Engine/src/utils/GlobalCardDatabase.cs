using System.Reflection;

namespace ScriptsOfTribute;

public class GlobalCardDatabase
{
    private static ThreadLocal<CardDatabase>? _instance;

    public static CardDatabase Instance
    {
        get
        {
            if (_instance != null) return _instance.Value;

            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "TalesOfTribute.cards.json";
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new InvalidOperationException($"Cant find resource'{resourceName}' in DLL. Available resources: {string.Join(", ", assembly.GetManifestResourceNames())}");
            }

            using StreamReader reader = new StreamReader(stream);
            string data = reader.ReadToEnd();
            var parser = new Parser(data);
            _instance = new ThreadLocal<CardDatabase>(() => new CardDatabase(parser.CreateAllCards()));

            return _instance.Value;
        }
    }

    private GlobalCardDatabase()
    {
    }
}
