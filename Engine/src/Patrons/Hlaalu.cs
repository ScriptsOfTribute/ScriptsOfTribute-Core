namespace TalesOfTribute
{
    public class Hlaalu : Patron
    {
        public override PlayResult PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Sacrifice 1 card you own in play that cost 1 or more Coin ->
             * Gain Prestige equal to the card's cost minus 1
             * 
             * Neutral:
             * Sacrifice 1 card you own in play that cost 1 or more Coin ->
             * Gain Prestige equal to the card's cost minus 1
             * 
             * Unfavored:
             * Sacrifice 1 card you own in play that cost 1 or more Coin ->
             * Gain Prestige equal to the card's cost minus 1
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return new Failure("Player has no card with cost >= 1, can't activate Hlaalu");
            }

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            List<Card> cardsInPlay = activator.Hand.Concat(activator.Played).ToList();
            return new Choice<Card>(cardsInPlay,
                cards =>
                {
                    var card = cards.First();
                    activator.Hand.Remove(card);
                    activator.PrestigeAmount += card.Cost - 1;
                    return new Success();
                },
                new ChoiceContext(this));
        }

        public override PlayResult PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return new Success();
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.GOODS_SHIPMENT };
        }

        public override PatronId PatronID => PatronId.HLAALU;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.GetAllPlayersCards().FindAll(card => card.Cost >= 1).Any();
        }
    }
}
