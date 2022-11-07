namespace TalesOfTribute
{
    public class DukeOfCrows : Patron
    {
        public override bool PatronActivation(Player activator, Player enemy, Card? card = null)
        {
            /*
             * Neutral:
             * Pay all your Coin -> Gain Power equal to Coin paid minus 1
             * 
             * Unfavored:
             * Pay all your Coin -> Gain Power equal to Coin paid minus 1
             */
            if (FavoredPlayer == activator.ID)
            {
                return true;
            }

            if (activator.CoinsAmount <= 0)
            {
                return false;
            }

            activator.PowerAmount += activator.CoinsAmount - 1;
            activator.CoinsAmount = 0;

            if (FavoredPlayer == -1)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = -1;

            return true;
        }

        public override bool PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return true;
        }
    }
}
