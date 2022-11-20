namespace TalesOfTribute
{
    public class Orgnum : Patron
    {
        public override PlayResult PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Pay 3 Coin -> Gain 1 Power for every 4 cards you own, rounded down.
             * Create 1 Maormer Boarding Party card and place it in your cooldown pile.
             * 
             * Neutral:
             * Pay 3 Coin -> Gain 1 Power for every 6 cards that you own, rounded down.
             * 
             * Unfavored:
             * Pay 3 Coin -> Gain 2 Power.
             */

            if (activator.CoinsAmount < 3)
            {
                return new Failure("Not enough Coin to activate Orgnum");
            }

            activator.CoinsAmount -= 3;

            int ownerCardsAmount = activator.GetAllPlayersCards().Count;

            if (FavoredPlayer == activator.ID) // Favored
            {
                activator.PowerAmount += ownerCardsAmount / 4;
                activator.CooldownPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.MAORMER_BOARDING_PARTY));
            }
            else if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED) // Neutral
                activator.PowerAmount += ownerCardsAmount / 6;
            else // Unfavored
                activator.PowerAmount += 2;

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
            return new List<CardId>() { CardId.SEA_ELF_RAID };
        }

        public override PatronId PatronID => PatronId.ORGNUM;
    }
}
