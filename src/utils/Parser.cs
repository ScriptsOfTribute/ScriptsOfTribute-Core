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

        public Parser()
        {
            JsonDocument doc = JsonDocument.Parse(cards_config.CARDS_JSON);
            this.root = doc.RootElement;
        }

        public List<Card> GetCardsByDeck(string[] Decks)
        {
            List<Card> collectedCards = new List<Card>();            
            var cardsEnumerator = this.root.EnumerateArray();

            foreach(var card in cardsEnumerator)
            {
                string deck = card.GetProperty("Deck").ToString();
                if (Decks.Contains(deck))
                {
                    collectedCards.Add(CreateCard(card));
                }
                
            }

            return collectedCards;
        }

        public Card CreateCard(JsonElement card)
        {
            int id = card.GetProperty("id").GetInt32();
            string name = card.GetProperty("Name").ToString();
            string deck = card.GetProperty("Deck").ToString();
            int cost = card.GetProperty("Cost").GetInt32();
            CardType type = ParseCardType(card.GetProperty("Type").ToString());
            int hp = card.GetProperty("HP").GetInt32();
            Effect[] effects = new Effect[4];

            string activation = card.GetProperty("Activation").ToString();
            effects[0] = ParseEffect(activation);
            string combo = card.GetProperty("Combo 2").ToString();
            effects[1] = ParseEffect(combo);
            combo = card.GetProperty("Combo 3").ToString();
            effects[2] = ParseEffect(combo);
            combo = card.GetProperty("Combo 4").ToString();
            effects[3] = ParseEffect(combo);


            return new Card(name, deck, id, cost, type, hp, effects, -1);
        }

        private Effect ParseEffect(string EffectToParse)
        {

            if (EffectToParse == "")
                return new Effect();

            string[] tokens = EffectToParse.Split(' ');
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

        private CardType ParseCardType(string CardTypeToParse)
        {
            switch (CardTypeToParse)
            {
                case "Action":
                    return CardType.Action;
                case "Agent":
                    return CardType.Agent;
                case "Contract Action":
                    return CardType.ContractAction;
                case "Starter":
                    return CardType.Starter;
                case "Contract Agent":
                    return CardType.ContractAgent;
                case "Curse":
                    return CardType.Curse;
                default:
                    return CardType.Action;
            }
        }
    }
}
