namespace TalesOfTribute
{
    public class Hlaalu : Patron
    {
        public PatronId ID = PatronId.HLAALU;
        
        public override bool PatronActivation(Player activator, Player enemy, Card? card = null)
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

            // TODO
            return true;
        }

        public override bool PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return true;
        }

        public override CardId GetStarterCard()
        {
            return CardId.GOODS_SHIPMENT;
        }
    }
}
