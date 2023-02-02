using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public class DukeOfCrows : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            /*
             * Neutral:
             * Pay all your Coin -> Gain Power equal to Coin paid minus 1
             * 
             * Unfavored:
             * Pay all your Coin -> Gain Power equal to Coin paid minus 1
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Coin to activate or player is already favored by Duke of Crows "), new List<CompletedAction>());
            }

            var powerToGain = activator.CoinsAmount - 1;
            activator.PowerAmount += powerToGain;
            var oldCoinsAmount = activator.CoinsAmount;
            activator.CoinsAmount = 0;

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return (new Success(), new List<CompletedAction>
            {
                new(activator.ID, CompletedActionType.GAIN_POWER, PatronID, powerToGain),
                new(activator.ID, CompletedActionType.GAIN_COIN, PatronID, -oldCoinsAmount),
            });
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // No benefits
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.PECK };
        }

        public override PatronId PatronID => PatronId.DUKE_OF_CROWS;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.CoinsAmount > 0 && FavoredPlayer != activator.ID;
        }
    }
}
