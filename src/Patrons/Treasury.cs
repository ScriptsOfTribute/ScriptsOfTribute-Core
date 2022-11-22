namespace TalesOfTribute
{
    public class Treasury : Patron
    {
        public override PlayResult PatronActivation(Player activator, Player enemy)
        {
            if (!CanPatronBeActivated(activator, enemy))
            {
                return new Failure("Not enough Coin to activate Treasury");
            }
            
            activator.CoinsAmount -= 2;
            List<Card> usedCards = activator.Played.Concat(activator.CooldownPile).ToList();
        
            // not sure how to aproach that
            return new Choice<Card>(usedCards,
                choices =>
            {
                activator.CooldownPile.Remove(choices.First());
                activator.DrawPile.Add(choices.First());
                return new Success();
            });
            //
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

        public override PatronId PatronID => PatronId.TREASURY;

        public override bool CanPatronBeActivated(Player activator, Player enemy){
            List<Card> usedCards = activator.Played.Concat(activator.CooldownPile).ToList();
            return activator.CoinsAmount>=2 && usedCards.Any();
        }
    }
}
