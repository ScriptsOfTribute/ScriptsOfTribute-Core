using TalesOfTribute.Board;

namespace TalesOfTribute
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

            enemy.CooldownPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.BEWILDERMENT));

            activator.CoinsAmount -= 3;

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return (new Success(),
                    new List<CompletedAction>
                    {
                        new(CompletedActionType.GAIN_COIN, PatronID, -3),
                        new(CompletedActionType.ADD_BEWILDERMENT_TO_OPPONENT, PatronID, 1),
                    });
        }

        public override ISimpleResult PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return new Success();
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
