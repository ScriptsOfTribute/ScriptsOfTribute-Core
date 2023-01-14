using TalesOfTribute.Board.Cards;

namespace TalesOfTribute
{
    public class Tavern : ITavern
    {
        public List<UniqueCard> Cards { get; set; }
        public List<UniqueCard> AvailableCards { get; set; }
        private readonly bool _simulationState = false;

        public Tavern(List<UniqueCard> cards, SeededRandom rnd)
        {
            AvailableCards = new List<UniqueCard>(5);
            Cards = cards.OrderBy(x => rnd.Next()).ToList();
        }

        public void SetUp(SeededRandom rnd)
        {
            Cards = Cards.OrderBy(_ => rnd.Next()).ToList();

            for (int i = 0; i < AvailableCards.Capacity; i++)
            {
                AvailableCards.Add(Cards.First());
                Cards.RemoveAt(0);
            }
        }

        public UniqueCard Acquire(UniqueCard card)
        {
            if (!AvailableCards.Contains(card))
            {
                throw new EngineException($"Card {card.CommonId} is not available!");
            }
            int idx = AvailableCards.FindIndex(x => x.UniqueId == card.UniqueId);
            AvailableCards.RemoveAt(idx);
            if (_simulationState && Cards.First().CommonId != CardId.UNKNOWN)
            {
                AvailableCards.Insert(idx, GlobalCardDatabase.Instance.GetCard(CardId.UNKNOWN));
            }
            else
            {
                AvailableCards.Insert(idx, this.Cards.First());
            }
            Cards.RemoveAt(0);
            return card;
        }

        public List<UniqueCard> GetAffordableCards(int coin)
        {
            return this.AvailableCards.Where(card => card.CommonId != CardId.UNKNOWN && card.Cost <= coin).ToList();
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

        private Tavern(List<UniqueCard> cards, List<UniqueCard> availableCards, bool cheats)
        {
            Cards = cards;
            AvailableCards = availableCards;
            _simulationState = !cheats;
        }

        public static Tavern FromSerializedBoard(SerializedBoard serializedBoard)
        {
            var tavernCards = new List<UniqueCard>(serializedBoard.TavernCards.Count);
            tavernCards.AddRange(serializedBoard.TavernCards);
            var tavernAvailableCards = new List<UniqueCard>(serializedBoard.TavernAvailableCards.Count);
            tavernAvailableCards.AddRange(serializedBoard.TavernAvailableCards);
            return new Tavern(tavernCards, tavernAvailableCards, serializedBoard.Cheats);
        }
    }
}
