using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public class Rajhin : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Pay 3 Coin -> Put 1 Bewilderment (empty/useless) card
             * in your opponent's cooldown pile
             * 
             * Neutral:
             * Pay 3 Coin -> Put 1 Bewilderment (empty/useless) card
             * in your opponent's cooldown pile
             * 
             * Unfavored:
             * Pay 3 Coin -> Put 1 Bewilderment (empty/useless) card
             * in your opponent's cooldown pile
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Coin to activate Rajhin"), new List<CompletedAction>());
            }
            var bewilderment = GlobalCardDatabase.Instance.GetCard(CardId.BEWILDERMENT);
            enemy.CooldownPile.Add(bewilderment);

            activator.CoinsAmount -= 3;

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return (new Success(),
                    new List<CompletedAction>
                    {
                        new(activator.ID, CompletedActionType.GAIN_COIN, PatronID, -3),
                        new(activator.ID, CompletedActionType.ADD_BEWILDERMENT_TO_OPPONENT, PatronID, 1, bewilderment),
                    });
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // No benefits
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.SWIPE };
        }

        public override PatronId PatronID => PatronId.RAJHIN;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.CoinsAmount >= 3;
        }
    }
}
