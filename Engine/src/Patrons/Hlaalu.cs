using ScriptsOfTribute.Board;

namespace ScriptsOfTribute
{
    public class Hlaalu : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Sacrifice 1 card from your hand or your played cards that cost 1 or more Coin  ->
             * Gain Prestige equal to the card's cost minus 1
             * 
             * Neutral:
             * Sacrifice 1 card from your hand or your played cards that cost 1 or more Coin  ->
             * Gain Prestige equal to the card's cost minus 1
             * 
             * Unfavored:
             * Sacrifice 1 card from your hand or your played cards that cost 1 or more Coin  ->
             * Gain Prestige equal to the card's cost minus 1
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Player has no card with cost >= 1, can't activate Hlaalu"), new List<CompletedAction>());
            }

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            var cardsInPlay = activator.Hand
                .Concat(activator.Played)
                .Concat(activator.AgentCards)
                .Where(c => c.Cost >= 1)
                .ToList();
            return (new Choice(cardsInPlay,
                ChoiceFollowUp.COMPLETE_HLAALU,
                new ChoiceContext(PatronID), 1, 1), new List<CompletedAction>());
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // No benefits
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.GOODS_SHIPMENT };
        }

        public override PatronId PatronID => PatronId.HLAALU;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.Hand
                .Concat(activator.Played)
                .Concat(activator.AgentCards)
                .ToList()
                .FindAll(card => card.Cost >= 1)
                .Any();
        }
    }
}
