using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute
{
    public class Pelin : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Pay 2 Power -> Return an Agent from your Cooldown pile to the top of your deck
             * 
             * Neutral:
             * Pay 2 Power -> Return an Agent from your Cooldown pile to the top of your deck
             * 
             * Unfavored:
             * Pay 2 Power -> Return an Agent from your Cooldown pile to the top of your deck
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Power to activate Pelin"), new List<CompletedAction>());
            }

            activator.PowerAmount -= 2;

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            var agentsInCooldownPile = activator.CooldownPile.FindAll(card => card.Type == CardType.AGENT);

            return (new Choice(agentsInCooldownPile,
                    ChoiceFollowUp.COMPLETE_PELLIN,
                    new ChoiceContext(PatronID), 1, 1),
                new List<CompletedAction>
                {
                    new(activator.ID, CompletedActionType.GAIN_POWER, PatronID, -2)
                });
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // No benefits
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.FORTIFY };
        }

        public override PatronId PatronID => PatronId.PELIN;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.PowerAmount >= 2 && activator.CooldownPile.FindAll(card => card.Type == CardType.AGENT).Any();
        }
    }
}