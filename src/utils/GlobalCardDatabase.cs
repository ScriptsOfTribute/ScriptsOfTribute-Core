namespace TalesOfTribute;

public class GlobalCardDatabase
{
    private static CardDatabase? _instance;

    public static CardDatabase Instance
    {
        get
        {
            if (_instance != null) return _instance;

            var data = File.ReadAllText("cards.json");
            var parser = new Parser(data);
            _instance = new CardDatabase(parser.CreateAllCards());

            return _instance;
        }
    }

    private GlobalCardDatabase()
    {
    }
}
