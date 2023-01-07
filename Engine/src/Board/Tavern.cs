using TalesOfTribute.Board.Cards;

namespace TalesOfTribute
{
    public class Tavern : ITavern
    {
        public List<UniqueCard> Cards { get; set; }
        public List<UniqueCard> AvailableCards { get; set; }

        public Tavern(List<UniqueCard> cards, SeededRandom rnd)
        {
            AvailableCards = new List<UniqueCard>(5);
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
            AvailableCards = new List<UniqueCard>(5);
        }

        public UniqueCard Acquire(UniqueCard card)
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

        public List<UniqueCard> GetAffordableCards(int coin)
        {
            return this.AvailableCards.Where(card => card.Cost <= coin).ToList();
        }

        public void ReplaceCard(UniqueCard toReplace)
        {
            UniqueCard newCard = Cards.First();
            int idx = AvailableCards.IndexOf(toReplace);
            Cards.Remove(newCard);
            Cards.Add(toReplace);
            AvailableCards.Remove(toReplace);
            AvailableCards.Insert(idx, newCard);
        }

        private Tavern(List<UniqueCard> cards, List<UniqueCard> availableCards)
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
