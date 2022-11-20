namespace TalesOfTribute
{
    public class DukeOfCrows : Patron
    {
        public override PlayResult PatronActivation(Player activator, Player enemy)
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
                return new Success();
            }

            if (activator.CoinsAmount <= 0)
            {
                return new Failure("Not enough Coin to activate Duke of Crows");
            }

            activator.PowerAmount += activator.CoinsAmount - 1;
            activator.CoinsAmount = 0;

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

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.PECK };
        }

        public override PatronId PatronID => PatronId.DUKE_OF_CROWS;
    }
}
