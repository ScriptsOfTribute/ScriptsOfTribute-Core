namespace ScriptsOfTribute;

public class GlobalCardDatabase
{
    private static ThreadLocal<CardDatabase>? _instance;

    public static CardDatabase Instance
    {
        get
        {
            if (_instance != null) return _instance.Value;

            var data = File.ReadAllText("cards.json");
            var parser = new Parser(data);
            _instance = new ThreadLocal<CardDatabase>(() => new CardDatabase(parser.CreateAllCards()));

            return _instance.Value;
        }
    }

    private GlobalCardDatabase()
    {
    }
}
