using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public class SaintAlessia : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Pay 4 Coin -> Create 1 Chainbreaker Sergeant card and place it in your cooldown pile
             * 
             * Neutral:
             * Pay 4 Coin ->  Create 1 Soldier of the Empire card and place it in your cooldown pile
             * 
             * Unfavored:
             * Pay 4 Coin -> Gain 2 power
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Coin to activate Saint Alessia"), new List<CompletedAction>());
            }

            activator.CoinsAmount -= 4;
            var actionList = new List<CompletedAction>
            {
                new(activator.ID, CompletedActionType.GAIN_COIN, PatronID, -4),
            };

            if (FavoredPlayer == activator.ID) // Favored
            {
                activator.CooldownPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.CHAINBREAKER_SERGEANT));
                actionList.Add(new CompletedAction(activator.ID, CompletedActionType.ADD_CHAINBREAKER_SERGEANT, PatronID, 1));
            }
            else if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED) // Neutral
            {
                activator.CooldownPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.SOLDIER_OF_THE_EMPIRE));
                actionList.Add(new CompletedAction(activator.ID, CompletedActionType.ADD_SOLDIER_OF_THE_EMPIRE, PatronID, 1));
            }
            else // Unfavored
            {
                activator.PowerAmount += 2;
                actionList.Add(new(activator.ID, CompletedActionType.GAIN_POWER, PatronID, 2));
            }

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return (new Success(), actionList);
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // No benefits
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.ALESSIAN_REBEL };
        }

        public override PatronId PatronID => PatronId.SAINT_ALESSIA;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.CoinsAmount >= 4;
        }
    }
}
