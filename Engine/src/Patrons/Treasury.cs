using TalesOfTribute.Board;

namespace TalesOfTribute
{
    public class Treasury : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Coin to activate Treasury"), new List<CompletedAction>());
            }

            activator.CoinsAmount -= 2;
            List<Card> usedCards = activator.Played.Concat(activator.CooldownPile).ToList();

            return (new Choice(usedCards,
                ChoiceFollowUp.COMPLETE_TREASURY,
                new ChoiceContext(PatronID), 1, 1),
                    new List<CompletedAction>
                    {
                        new(CompletedActionType.GAIN_COIN, PatronID, -2),
                    });
        }

        public override ISimpleResult PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return new Success();
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>()
            {
                CardId.GOLD,
                CardId.GOLD,
                CardId.GOLD,
                CardId.GOLD,
                CardId.GOLD,
                CardId.GOLD
            };
        }

        public override PatronId PatronID => PatronId.TREASURY;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            List<Card> usedCards = activator.Played.Concat(activator.CooldownPile).ToList();
            return activator.CoinsAmount >= 2 && usedCards.Any();
        }
    }
}
