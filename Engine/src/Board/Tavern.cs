namespace TalesOfTribute
{
    public class Tavern : ITavern
    {
        public List<Card> Cards { get; set; }
        public List<Card> AvailableCards { get; set; }

        public Tavern(List<Card> cards, SeededRandom rnd)
        {
            AvailableCards = new List<Card>(5);
            Cards = cards.OrderBy(x => rnd.Next()).ToList();
        }

        public void DrawCards(SeededRandom rnd)
        {
            this.Cards = this.Cards.OrderBy(x => rnd.Next()).ToList();

            for (int i = 0; i < this.AvailableCards.Capacity; i++)
            {
                AvailableCards.Add(this.Cards.First());
                this.Cards.RemoveAt(0);
            }
        }

        public void ShuffleBack()
        {
            for (int i = 0; i < this.AvailableCards.Capacity; i++)
            {
                this.Cards.Add(this.AvailableCards[i]);
            }
            AvailableCards = new List<Card>(5);
        }

        public Card Acquire(Card card)
        {
            if (!AvailableCards.Contains(card))
            {
                throw new Exception($"Card {card.CommonId} is not available!");
            }
            int idx = AvailableCards.FindIndex(x => x.UniqueId == card.UniqueId);
            AvailableCards.RemoveAt(idx);
            AvailableCards.Insert(idx, this.Cards.First());
            Cards.RemoveAt(0);
            return card;
        }

        public List<Card> GetAffordableCards(int coin)
        {
            return this.AvailableCards.Where(card => card.Cost <= coin).ToList();
        }

        public void ReplaceCard(Card toReplace)
        {
            Card newCard = Cards.First();
            Cards.Remove(newCard);
            Cards.Add(toReplace);
            AvailableCards.Remove(toReplace);
            AvailableCards.Add(newCard);
        }

        private Tavern(List<Card> cards, List<Card> availableCards)
        {
            Cards = cards;
            AvailableCards = availableCards;
        }

        public static Tavern FromSerializedBoard(SerializedBoard serializedBoard)
        {
            return new Tavern(serializedBoard.TavernCards.ToList(), serializedBoard.TavernAvailableCards.ToList());
        }
    }
}
