using Newtonsoft.Json.Linq;

namespace TalesOfTribute
{
    public class Parser
    {
        private const string InvalidJsonMessage = "Invalid cards.json format!";
        readonly JArray _root;

        public Parser(string data)
        {
            this._root = JArray.Parse(data);
        }

        public IEnumerable<Card> CreateAllCards()
        {
            return from card in _root.Children<JObject>() select CreateCard(card);
        }

        private Card CreateCard(JObject card)
        {
            var id = card["id"]?.ToObject<CardId>() ?? throw new Exception(InvalidJsonMessage);
            string name = card["Name"]?.ToObject<string>() ?? throw new Exception(InvalidJsonMessage);
            PatronId deck = Patron.IdFromString(card["Deck"]?.ToObject<string>() ?? throw new Exception(InvalidJsonMessage));
            int cost = card["Cost"]?.ToObject<int>() ?? throw new Exception(InvalidJsonMessage);
            CardType type = ParseCardType(card["Type"]?.ToObject<string>() ?? throw new Exception(InvalidJsonMessage));
            int hp = card["HP"]?.ToObject<int>() ?? throw new Exception(InvalidJsonMessage);
            ComplexEffect?[] effects = new ComplexEffect?[4];
            bool taunt = card["Taunt"]?.ToObject<bool>() ?? false;

            CardId? family = card["Family"]?.ToObject<CardId?>();

            var activation = card["Activation"]?.ToObject<string>();
            effects[0] = ParseEffect(activation);
            var combo = card["Combo 2"]?.ToObject<string>();
            effects[1] = ParseEffect(combo);
            combo = card["Combo 3"]?.ToObject<string>();
            effects[2] = ParseEffect(combo);
            combo = card["Combo 4"]?.ToObject<string>();
            effects[3] = ParseEffect(combo);

            return new Card(name, deck, id, cost, type, hp, effects, -1, family, taunt);
        }

        private ComplexEffect? ParseEffect(string? effectToParse)
        {

            if (effectToParse == null)
                return null;

            string[] tokens = effectToParse.Split(' ');

            return tokens.Length switch
            {
                2 => new Effect(Effect.MapEffectType(tokens[0]), Int32.Parse(tokens[1])),
                5 when tokens[2] == "OR" => new EffectOr(
                    new Effect(Effect.MapEffectType(tokens[0]), Int32.Parse(tokens[1])),
                    new Effect(Effect.MapEffectType(tokens[3]), Int32.Parse(tokens[4]))),
                5 when tokens[2] == "AND" => new EffectComposite(
                    new Effect(Effect.MapEffectType(tokens[0]), Int32.Parse(tokens[1])),
                    new Effect(Effect.MapEffectType(tokens[3]), Int32.Parse(tokens[4]))),
                _ => throw new Exception(InvalidJsonMessage)
            };
        }

        private CardType ParseCardType(string cardTypeToParse)
        {
            return cardTypeToParse switch
            {
                "Action" => CardType.ACTION,
                "Agent" => CardType.AGENT,
                "Contract Action" => CardType.CONTRACT_ACTION,
                "Starter" => CardType.STARTER,
                "Contract Agent" => CardType.CONTRACT_AGENT,
                "Curse" => CardType.CURSE,
                _ => CardType.ACTION
            };
        }
    }
}
