namespace TalesOfTribute
{
    public class Psijic : Patron
    {
        public PatronId ID = PatronId.PSIJIC;
        
        public override bool PatronActivation(Player activator, Player enemy, Card? card = null)
        {
            /*
             * Favored:
             * Opponent has 1 agent card active. Pay 4 Coin ->
             * Knock Out 1 of enemy's agents into cooldown pile
             * 
             * Neutral:
             * Opponent has 1 agent card active. Pay 4 Coin ->
             * Knock Out 1 of enemy's agents into cooldown pile
             * 
             * Unfavored:
             * Opponent has 1 agent card active. Pay 4 Coin ->
             * Knock Out 1 of enemy's agents into cooldown pile
             */

            if (activator.CoinsAmount < 4 || enemy.Agents.Count <= 0 || card == null)
            {
                return false;
            }

            activator.CoinsAmount -= 4;

            enemy.Agents.Remove((Card)card);
            enemy.CooldownPile.Add((Card)card);

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
            return CardId.MAINLAND_INQUIRIES;
        }
    }
}
