namespace TalesOfTribute
{
    public class Rajhin : Patron
    {
        public PatronId ID = PatronId.RAJHIN;
        
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

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return true;
        }

        public override bool PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return true;
        }

        public override CardId GetStarterCard()
        {
            return CardId.SWIPE;
        }
    }
}
