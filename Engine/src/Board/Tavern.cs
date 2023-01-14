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

        public List<UniqueCard> GetAffordableCards(int coin)
        {
            return this.AvailableCards.Where(card => card.CommonId != CardId.UNKNOWN && card.Cost <= coin).ToList();
        }

        public void ReplaceCard(UniqueCard toReplace)
        {
            DrawAt(RemoveCard(toReplace));
        }

        public int RemoveCard(UniqueCard card)
        {
            if (!AvailableCards.Contains(card))
            {
                throw new EngineException($"Card {card.CommonId} is not available!");
            }
            var idx = AvailableCards.IndexOf(card);
            AvailableCards.Remove(card);
            return idx;
        }

        public void DrawAt(int index)
        {
            
            if (_simulationState && Cards.First().CommonId != CardId.UNKNOWN)
            {
                AvailableCards.Insert(index, GlobalCardDatabase.Instance.GetCard(CardId.UNKNOWN));
            }
            else
            {
                AvailableCards.Insert(index, this.Cards.First());
            }
            Cards.RemoveAt(0);
        }

        private Tavern(List<UniqueCard> cards, List<UniqueCard> availableCards, bool cheats)
        {
            Cards = cards;
            AvailableCards = availableCards;
            _simulationState = !cheats;
        }

        public static Tavern FromSerializedBoard(SerializedBoard serializedBoard)
        {
            return new Tavern(serializedBoard.TavernCards.ToList(), serializedBoard.TavernAvailableCards.ToList(), serializedBoard.Cheats);
        }
    }
}
