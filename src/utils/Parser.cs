using System.Text.Json;

namespace TalesOfTribute
{
    public class Parser
    {
        public JsonElement root;

        public Parser(string data)
        {
            JsonDocument doc = JsonDocument.Parse(data);
            this.root = doc.RootElement;
        }

        public IEnumerable<Card> CreateAllCards()
        {
            var cardsEnumerator = this.root.EnumerateArray();
            return from card in cardsEnumerator select CreateCard(card);
        }

        private Card CreateCard(JsonElement card)
        {
            var id = (CardId)card.GetProperty("id").GetInt32();
            string name = card.GetProperty("Name").ToString();
            PatronId deck = Patron.IdFromString(card.GetProperty("Deck").ToString());
            int cost = card.GetProperty("Cost").GetInt32();
            CardType type = ParseCardType(card.GetProperty("Type").ToString());
            int hp = card.GetProperty("HP").GetInt32();
            ComplexEffect?[] effects = new ComplexEffect?[4];

            CardId? family = null;
            if (card.TryGetProperty("Family", out var familyElement))
            {
                family = (CardId?)familyElement.GetInt32();
            }

            string activation = card.GetProperty("Activation").ToString();
            effects[0] = ParseEffect(activation);
            string combo = card.GetProperty("Combo 2").ToString();
            effects[1] = ParseEffect(combo);
            combo = card.GetProperty("Combo 3").ToString();
            effects[2] = ParseEffect(combo);
            combo = card.GetProperty("Combo 4").ToString();
            effects[3] = ParseEffect(combo);


            return new Card(name, deck, id, cost, type, hp, effects, -1, family);
        }

        private ComplexEffect? ParseEffect(string effectToParse)
        {

            if (effectToParse == "")
                return null;

            string[] tokens = effectToParse.Split(' ');

            if (tokens.Length == 2)
            {
                return new Effect(Effect.MapEffectType(tokens[0]), Int32.Parse(tokens[1]));
            }
            else if (tokens.Length == 5)
            {
                if (tokens[2] == "OR")
                {
                    return new EffectChoice(
                        new Effect(Effect.MapEffectType(tokens[0]), Int32.Parse(tokens[1])),
                        new Effect(Effect.MapEffectType(tokens[3]), Int32.Parse(tokens[4])
                        )
                    );
                }
                else if (tokens[2] == "AND")
                {
                    return new EffectComplex(
                        new Effect(Effect.MapEffectType(tokens[0]), Int32.Parse(tokens[1])),
                        new Effect(Effect.MapEffectType(tokens[3]), Int32.Parse(tokens[4])
                        )
                    );
                }
                else
                {
                    throw new Exception("Invalid cards.json format!");
                }
            }

            return null;
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
