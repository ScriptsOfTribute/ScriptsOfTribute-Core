namespace TalesOfTribute
{
    public class Ansei : Patron
    {
        public PatronId ID = PatronId.ANSEI;

        public override bool PatronActivation(Player activator, Player enemy, Card? card = null)
        {
            /*
             * Neutral & Unfavored
             * Pay 2 power -> Gain 1 Coin. This Patron now favors you
             */

            if (FavoredPlayer == activator.ID)
            {
                return true;
            }

            if (activator.PowerAmount < 2)
            {
                return false;
            }

            activator.PowerAmount -= 2;
            activator.CoinsAmount += 1;

            FavoredPlayer = activator.ID;

            return true;
        }

        public override bool PatronPower(Player activator, Player enemy)
        {
            // Gain 1 Coin at the start of your turn

            if (FavoredPlayer != activator.ID)
            {
                return false;
            }

            activator.CoinsAmount += 1;

            return true;
        }

        public override CardId GetStarterCard()
        {
            return CardId.WAY_OF_THE_SWORD;
        }
    }
}
