using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            List<CardId> result = this._tavern.GetAffordableCards(6);

            Assert.Contains(
                CardId.LUXURY_EXPORTS,
                result
            );

            Assert.Contains(
                CardId.OATHMAN,
                result
            );

            Assert.Contains(
                CardId.CONQUEST,
                result
            );

            Assert.DoesNotContain(
                CardId.CURRENCY_EXCHANGE,
                result
            );

            result = this._tavern.GetAffordableCards(10);

            Assert.Contains(
                CardId.ANSEIS_VICTORY,
                result
            );

        }

        [Fact]
        public void ReplaceCardTest()
        {
            this._tavern.AvailableCards = new List<Card>()
            {
                GlobalCardDatabase.Instance.GetCard(CardId.CURRENCY_EXCHANGE),
                GlobalCardDatabase.Instance.GetCard(CardId.LUXURY_EXPORTS),
                GlobalCardDatabase.Instance.GetCard(CardId.OATHMAN),
                GlobalCardDatabase.Instance.GetCard(CardId.CONQUEST),
                GlobalCardDatabase.Instance.GetCard(CardId.ANSEIS_VICTORY)
            };

            Assert.Contains(
                CardId.OATHMAN,
                this._tavern.AvailableCards.Select(card => card.Id)
            );
            this._tavern.ReplaceCard(CardId.OATHMAN);

            Assert.DoesNotContain(
                CardId.OATHMAN,
                this._tavern.AvailableCards.Select(card => card.Id)
            );

            Assert.Equal(5, this._tavern.AvailableCards.Count);

        }
    }
}
