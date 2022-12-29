using TalesOfTribute.Board;

namespace TalesOfTribute
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
            activator.Draw();

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return (new Success(),
                    new List<CompletedAction>
                    {
                        new(CompletedActionType.GAIN_POWER, PatronID, -2),
                        new(CompletedActionType.DRAW, PatronID, 1),
                    });
        }

        public override ISimpleResult PatronPower(Player activator, Player enemy)
        {
            // No benefits
            return new Success();
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