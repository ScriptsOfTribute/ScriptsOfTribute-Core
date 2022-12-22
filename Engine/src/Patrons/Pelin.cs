namespace TalesOfTribute
{
    public class Pelin : Patron
    {
        public override PlayResult PatronActivation(Player activator, Player enemy)
        {
            /*
             * Favored:
             * Pay 2 Power -> Return an Agent from your Cooldown pile to the top of your deck
             * 
             * Neutral:
             * Pay 2 Power -> Return an Agent from your Cooldown pile to the top of your deck
             * 
             * Unfavored:
             * Pay 2 Power -> Return an Agent from your Cooldown pile to the top of your deck
             */

            if (!CanPatronBeActivated(activator, enemy))
            {
                return new Failure("Not enough Power to activate Pelin");
            }

            activator.PowerAmount -= 2;

            if (FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                FavoredPlayer = activator.ID;
            else if (FavoredPlayer == enemy.ID)
                FavoredPlayer = PlayerEnum.NO_PLAYER_SELECTED;

            List<Card> agentsInCooldownPile = activator.CooldownPile.FindAll(card => card.Type == CardType.AGENT);

            return new Choice<Card>(agentsInCooldownPile,
                (choices, complexChoiceExecutor) =>
                {
                    var choice = choices.First();
                    return complexChoiceExecutor.CompletePelin(choice);
                },
                new ChoiceContext(this), 1, 1);
        }

        public override ISimpleResult PatronPower(Player activator, Player enemy)
        {
            // No benefits
            return new Success();
        }

        public override List<CardId> GetStarterCards()
        {
            return new List<CardId>() { CardId.FORTIFY };
        }

        public override PatronId PatronID => PatronId.PELIN;

        public override bool CanPatronBeActivated(Player activator, Player enemy)
        {
            return activator.PowerAmount >= 2 && activator.CooldownPile.FindAll(card => card.Type == CardType.AGENT).Any();
        }
    }
}