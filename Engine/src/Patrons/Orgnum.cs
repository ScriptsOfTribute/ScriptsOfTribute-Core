using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public class Orgnum : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Pay 3 Coin -> Gain 1 Power for every 4 cards you own, rounded down.
             * Create 1 Summerset Sacking card and place it in your cooldown pile.
             * 
             * Neutral:
             * Pay 3 Coin -> Gain 1 Power for every 6 cards that you own, rounded down.
             * 
             * Unfavored:
             * Pay 3 Coin -> Gain 2 Power.
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Coin to activate Orgnum"), new List<CompletedAction>());
            }

            activator.CoinsAmount -= 3;

            var ownerCardsAmount = activator.GetAllPlayersCards().Count;

            int powerGained;
            if (FavoredPlayer == activator.ID) // Favored
            {
                powerGained = ownerCardsAmount / 4;
                activator.PowerAmount += powerGained;
                activator.CooldownPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.SUMMERSET_SACKING));
            }
            else if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED) // Neutral
            {
                powerGained = ownerCardsAmount / 6;
                activator.PowerAmount += powerGained;
            }
            else // Unfavored
            {
                powerGained = 2;
                activator.PowerAmount += powerGained;
            }
            
            var actionList = new List<CompletedAction>
            {
                new(activator.ID, CompletedActionType.GAIN_COIN, PatronID, -3),
                new(activator.ID, CompletedActionType.GAIN_POWER, PatronID, powerGained),

            };

            if (FavoredPlayer == activator.ID)
            {
                actionList.Add(new CompletedAction(activator.ID, CompletedActionType.ADD_SUMMERSET_SACKING, PatronID, 1));
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
            return new List<CardId>() { CardId.SEA_ELF_RAID };
        }

        public override PatronId PatronID => PatronId.ORGNUM;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.CoinsAmount >= 3;
        }
    }
}
