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

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            return new Choice<CardId>(activator.Hand.Select(c => c.Id).ToList(),
                cards =>
                {
                    var card = activator.Hand.First(c => c.Id == cards.First());
                    if (card.Cost < 1)
                        return new Failure("Card's Cost is too small to use Hlaalu");
                    activator.Hand.Remove(card);
                    activator.PrestigeAmount += card.Cost - 1;
                    return new Success();
                });
        }

        public override PlayResult PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return new Success();
        }

        public override CardId GetStarterCard()
        {
            return CardId.GOODS_SHIPMENT;
        }
    }
}
