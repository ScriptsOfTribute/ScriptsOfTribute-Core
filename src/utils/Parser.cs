using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        public List<Card> GetCardsByDeck(string[] decks)
        {
            var cardsEnumerator = this.root.EnumerateArray();

            var collectedCards =
                from card in cardsEnumerator
                    let deck = card.GetProperty("Deck").ToString()
                    where decks.Contains(deck)
                    select CreateCard(card);

            return FilterOutPreUpgradeCards(collectedCards).ToList();
        }

        // TODO: Add ability for user to configure which card he wants to keep.
        // For now, we will keep cards after upgrade.
        private IEnumerable<Card> FilterOutPreUpgradeCards(IEnumerable<Card> cards)
        {
            var cardsEnumerable = cards.ToList();
            var families = cardsEnumerable
                .Where(card => card.Family >= 0)
                .Select(card => card.Family)
                .Distinct();
            // Pre-upgrade cards have the same ID as the family they are in.
            return from card in cardsEnumerable
                where !families.Contains(card.InstanceID) select card;
        }

        private Card CreateCard(JsonElement card)
        {
            int id = card.GetProperty("id").GetInt32();
            string name = card.GetProperty("Name").ToString();
            string deck = card.GetProperty("Deck").ToString();
            int cost = card.GetProperty("Cost").GetInt32();
            CardType type = ParseCardType(card.GetProperty("Type").ToString());
            int hp = card.GetProperty("HP").GetInt32();
            Effect[] effects = new Effect[4];

            int family = -1;
            if (card.TryGetProperty("Family", out var familyElement))
            {
                family = familyElement.GetInt32();
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

        private Effect ParseEffect(string effectToParse)
        {

            if (effectToParse == "")
                return new Effect();

            string[] tokens = effectToParse.Split(' ');
            Effect effect = new Effect();

            if (tokens.Length == 2)
            {
                effect = new Effect(Effect.MapEffectType(tokens[0]), Int32.Parse(tokens[1]));
            }
            else if (tokens.Length == 5)
            {
                effect = new Effect(
                    Effect.MapEffectType(tokens[0]), 
                    Int32.Parse(tokens[1]),
                    tokens[2],
                    Effect.MapEffectType(tokens[3]),
                    Int32.Parse(tokens[4])
                );
            }

            return effect;
        }

        private CardType ParseCardType(string cardTypeToParse)
        {
            return cardTypeToParse switch
            {
                "Action" => CardType.Action,
                "Agent" => CardType.Agent,
                "Contract Action" => CardType.ContractAction,
                "Starter" => CardType.Starter,
                "Contract Agent" => CardType.ContractAgent,
                "Curse" => CardType.Curse,
                _ => CardType.Action
            };
        }
    }
}
