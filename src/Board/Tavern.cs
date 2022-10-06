using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalesOfTribute
{
    public class Tavern
    {
        public List<Card> cards;
        public Card[] availableCards;

        public Tavern(string[] decks)
        {
            Parser parser = new Parser();
            this.cards = parser.GetCardsByDeck(decks);
            availableCards = new Card[5];
        }

        public void DrawCards()
        {
            Random rnd = new Random();
            this.cards = this.cards.OrderBy(x => rnd.Next(0, cards.Count)).ToList<Card>();
            
            for(int i = 0; i < this.availableCards.Length; i++)
            {
                availableCards.SetValue(this.cards.ElementAt(i), i);
                this.cards.RemoveAt(i);
            }
        }

        public void ShuffleBack()
        {
            for (int i = 0; i < this.availableCards.Length; i++)
            {
                Card? elem = this.availableCards[i];
                if (elem != null)
                {
                    this.cards.Add(elem);
                    this.availableCards.SetValue(null, i);
                }
            }
        }

        public Card? BuyCard(int idx)
        {
            if (idx >= this.availableCards.Length)
                return null;

            Card toReturn = this.availableCards[idx];
            this.availableCards.SetValue(null, idx);


            return toReturn;
        }
    }
}
