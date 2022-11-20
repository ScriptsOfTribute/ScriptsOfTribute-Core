namespace TalesOfTribute
{
    public class Psijic : Patron
    {
        public override PlayResult PatronActivation(Player activator, Player enemy)
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

            if (activator.CoinsAmount < 4)
                return new Failure("Not enough Coin to activate Psijic");
            if (enemy.Agents.Count <= 0)
                return new Failure("Enemy has no agents, can't activate Psijic");

            activator.CoinsAmount -= 4;

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return new Choice<Card>(enemy.Agents,
                choices =>
            {
                enemy.Agents.Remove(choices.First());
                enemy.CooldownPile.Add(choices.First());
                return new Success();
            });
        }

        public override PlayResult PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return new Success();
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.MAINLAND_INQUIRIES };
        }

        public override PatronId PatronID => PatronId.PSIJIC;
    }
}
