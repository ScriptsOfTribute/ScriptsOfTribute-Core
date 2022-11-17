namespace TalesOfTribute
{
    public class Rajhin : Patron
    {
        public override PlayResult PatronActivation(Player activator, Player enemy)
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
                return new Failure("Not enough Coin to activate Rajhin");
            }

            enemy.CooldownPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.BEWILDERMENT));

            activator.CoinsAmount -= 3;

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return new Success();
        }

        public override PlayResult PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return new Success();
        }

        public override CardId GetStarterCard()
        {
            return CardId.SWIPE;
        }
    }
}
