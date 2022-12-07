using TalesOfTribute;

namespace Tests.Board
{
    public class TavernTests
    {
        Tavern _tavern = new Tavern(
            GlobalCardDatabase.Instance.GetCardsByPatron(
                new[] { PatronId.HLAALU, PatronId.ANSEI }
            ));

        [Fact]
        public void TavernAffordableCardsTest()
        {
            this._tavern.AvailableCards = new List<Card>()
            {
                GlobalCardDatabase.Instance.GetCard(CardId.CURRENCY_EXCHANGE), // Cost 7
                GlobalCardDatabase.Instance.GetCard(CardId.LUXURY_EXPORTS), // Cost 2
                GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN), // Cost 6
                GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST), // Cost 4
                GlobalCardDatabase.Instance.GetCard(CardId.ANSEIS_VICTORY) // Cost 9
            };

            var result = this._tavern.GetAffordableCards(6).Select(card => card.CommonId).ToList();

            Assert.Contains(
                GlobalCardDatabase.Instance.GetCard(CardId.LUXURY_EXPORTS).CommonId,
                result
            );

            Assert.Contains(
                GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN).CommonId,
                result
            );

            Assert.Contains(
                GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST).CommonId,
                result
            );

            Assert.DoesNotContain(
                GlobalCardDatabase.Instance.GetCard(CardId.CURRENCY_EXCHANGE).CommonId,
                result
            );

            result = this._tavern.GetAffordableCards(10).Select(card => card.CommonId).ToList();

            Assert.Contains(
                GlobalCardDatabase.Instance.GetCard(CardId.ANSEIS_VICTORY).CommonId,
                result
            );

        }

        [Fact]
        public void ReplaceCardTest()
        {
            var cardToReplace = GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN);
            this._tavern.AvailableCards = new List<Card>()
            {
                GlobalCardDatabase.Instance.GetCard(CardId.CURRENCY_EXCHANGE),
                GlobalCardDatabase.Instance.GetCard(CardId.LUXURY_EXPORTS),
                cardToReplace,
                GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST),
                GlobalCardDatabase.Instance.GetCard(CardId.ANSEIS_VICTORY)
            };

            Assert.Contains(
                CardId.OATHMAN,
                this._tavern.AvailableCards.Select(card => card.CommonId)
            );
            this._tavern.ReplaceCard(cardToReplace);

            Assert.DoesNotContain(
                CardId.OATHMAN,
                this._tavern.AvailableCards.Select(card => card.CommonId)
            );

            Assert.Equal(5, this._tavern.AvailableCards.Count);
        }
    }
}
