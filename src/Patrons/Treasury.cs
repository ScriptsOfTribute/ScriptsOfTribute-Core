namespace TalesOfTribute
{
    public class Treasury : Patron
    {
        public override PlayResult PatronActivation(Player activator, Player enemy)
        {
            return new Success();
        }

        public override PlayResult PatronPower(Player activator, Player enemy)
        {
            // No benefits

            return new Success();
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>()
            {
                CardId.GOLD,
                CardId.GOLD,
                CardId.GOLD,
                CardId.GOLD,
                CardId.GOLD,
                CardId.GOLD
            };
        }

        public override PatronId GetPatronID()
        {
            return PatronId.TREASURY;
        }
    }
}
