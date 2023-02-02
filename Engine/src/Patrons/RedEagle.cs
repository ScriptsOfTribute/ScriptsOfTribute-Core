using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public class RedEagle : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Pay 2 Power -> Draw a card.
             * 
             * Neutral:
             * Pay 2 Power -> Draw a card.
             * 
             * Unfavored:
             * Pay 2 Power -> Draw a card.
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Power to activate Red Eagle"), new List<CompletedAction>());
            }

            activator.PowerAmount -= 2;
            // TODO: Consider adding drawn cards here? It's probably a good idea, but Tossed cards are added like this.
            activator.Draw(1);

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return (new Success(),
                    new List<CompletedAction>
                    {
                        new(activator.ID, CompletedActionType.GAIN_POWER, PatronID, -2),
                        new(activator.ID, CompletedActionType.DRAW, PatronID, 1),
                    });
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // No benefits
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.WAR_SONG };
        }

        public override PatronId PatronID => PatronId.RED_EAGLE;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.PowerAmount >= 2;
        }
    }
}