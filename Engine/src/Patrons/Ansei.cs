using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public class Ansei : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            /*
             * Neutral & Unfavored
             * Pay 2 power -> Gain 1 Coin. This Patron now favors you
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Power to activate Ansei"), new List<CompletedAction>());
            }

            activator.PowerAmount -= 2;
            activator.CoinsAmount += 1;

            FavoredPlayer = activator.ID;

            return (new Success(), new List<CompletedAction>
            {
                new(activator.ID, CompletedActionType.GAIN_POWER, PatronID, -2),
                new(activator.ID, CompletedActionType.GAIN_COIN, PatronID, 1),
            });
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // Gain 1 Coin at the start of your turn

            if (FavoredPlayer == activator.ID)
                activator.CoinsAmount += 1;
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.WAY_OF_THE_SWORD };
        }

        public override PatronId PatronID => PatronId.ANSEI;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.PowerAmount >= 2 && FavoredPlayer != activator.ID;
        }
    }
}
