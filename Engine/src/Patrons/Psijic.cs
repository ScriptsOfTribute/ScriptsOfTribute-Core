using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public class Psijic : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
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

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Coin or enemy has no agents, can't activate Psijic"), new List<CompletedAction>());
            }

            activator.CoinsAmount -= 4;

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;
            // We should check if there is any taunt agent
            return (new Choice(enemy.AgentCards,
                ChoiceFollowUp.COMPLETE_PSIJIC,
                new ChoiceContext(PatronID), 1, 1),
                    new List<CompletedAction>
                    {
                        new(activator.ID, CompletedActionType.GAIN_COIN, PatronID, -4),
                    });
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // No benefits
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.MAINLAND_INQUIRIES };
        }

        public override PatronId PatronID => PatronId.PSIJIC;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.CoinsAmount >= 4 && enemy.Agents.Any();
        }
    }
}
