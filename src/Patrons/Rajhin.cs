namespace TalesOfTribute
{
    public class Rajhin : Patron
    {
        public override bool PatronActivation(Player activator, Player enemy, Card? card = null)
        {
            /*
             * Favored:
             * Pay 3 Coin -> Put 1 Bewilderment (empty/useless) card
             * in your opponent's cooldown pile
             * 
             * Neutral:
             * Pay 3 Coin -> Put 1 Bewilderment (empty/useless) card
             * in your opponent's cooldown pile
             * 
             * Unfavored:
             * Pay 3 Coin -> Put 1 Bewilderment (empty/useless) card
             * in your opponent's cooldown pile
             */

            if (activator.CoinsAmount < 3)
            {
                return false;
            }

            enemy.CooldownPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.BEWILDERMENT));

            activator.CoinsAmount -= 3;

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
