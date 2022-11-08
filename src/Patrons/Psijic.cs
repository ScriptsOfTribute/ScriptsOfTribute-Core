namespace TalesOfTribute
{
    public class Psijic : Patron
    {
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
