using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute
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
            return this.AvailableCards.Where(card => card.Cost <= coin).ToList();
        }

        public void ReplaceCard(UniqueCard toReplace)
        {
            DrawAt(RemoveCard(toReplace));
            Cards.Add(toReplace);
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
            if (!Cards.Any())
            {
                return;
            }

            AvailableCards.Insert(index, this.Cards.First());
            Cards.RemoveAt(0);
        }

        private Tavern(List<UniqueCard> cards, List<UniqueCard> availableCards)
        {
            Cards = cards;
            AvailableCards = availableCards;
        }

        public static Tavern FromSerializedBoard(FullGameState fullGameState)
        {
            var tavernCards = new List<UniqueCard>(fullGameState.TavernCards.Count);
            tavernCards.AddRange(fullGameState.TavernCards);
            var tavernAvailableCards = new List<UniqueCard>(fullGameState.TavernAvailableCards.Count);
            tavernAvailableCards.AddRange(fullGameState.TavernAvailableCards);
            return new Tavern(tavernCards, tavernAvailableCards);
        }
    }
}
