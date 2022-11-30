namespace TalesOfTribute
{
    public class Tavern : ITavern
    {
        public List<Card> Cards { get; set; }
        public List<Card> AvailableCards { get; set; }
        private Random _rnd;

        public Tavern(List<Card> cards)
        {
            AvailableCards = new List<Card>(5);
            _rnd = new Random();
            Cards = cards.OrderBy(x => _rnd.Next(0, cards.Count)).ToList();
        }

        public void DrawCards()
        {
            this.Cards = this.Cards.OrderBy(x => this._rnd.Next(0, Cards.Count)).ToList();

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
                throw new Exception($"Card {card.Id} is not available!");
            }
            int idx = AvailableCards.FindIndex(x => x.Guid == card.Guid);
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
    }
}
