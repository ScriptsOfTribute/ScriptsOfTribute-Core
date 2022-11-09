namespace TalesOfTribute
{
    public class Tavern
    {
        public List<Card> Cards;
        public Card?[] AvailableCards;

        public Tavern(List<Card> cards)
        {
            Cards = cards;
            AvailableCards = new Card?[5];
        }

        public void DrawCards()
        {
            Random rnd = new Random();
            this.Cards = this.Cards.OrderBy(x => rnd.Next(0, Cards.Count)).ToList();

            for (int i = 0; i < this.AvailableCards.Length; i++)
            {
                AvailableCards.SetValue(this.Cards.ElementAt(i), i);
                this.Cards.RemoveAt(i);
            }
        }

        public void ShuffleBack()
        {
            for (int i = 0; i < this.AvailableCards.Length; i++)
            {
                Card? elem = this.AvailableCards[i];
                if (elem != null)
                {
                    this.Cards.Add((Card)elem);
                    this.AvailableCards.SetValue(null, i);
                }
            }
        }

        public Card? BuyCard(int idx)
        {
            if (idx >= this.AvailableCards.Length)
                return null;

            Card? toReturn = this.AvailableCards[idx];
            this.AvailableCards.SetValue(null, idx);


            return toReturn;
        }
    }
}
