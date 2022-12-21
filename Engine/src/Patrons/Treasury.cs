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

            return new Choice<Card>(usedCards,
                choices =>
                {
                    Card selectedCard = choices.First();
                    if (activator.Played.Contains(selectedCard))
                    {
                        activator.Played.Remove(selectedCard);
                        activator.DrawPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN));
                    }
                    else
                    {
                        activator.CooldownPile.Remove(selectedCard);
                        activator.DrawPile.Add(GlobalCardDatabase.Instance.GetCard(CardId.WRIT_OF_COIN));
                    }
                    return new Success();
                },
                new ChoiceContext(this), 1, 1);
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

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            List<Card> usedCards = activator.Played.Concat(activator.CooldownPile).ToList();
            return activator.CoinsAmount >= 2 && usedCards.Any();
        }
    }
}
