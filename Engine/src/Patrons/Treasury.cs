﻿using TalesOfTribute.Board;
using TalesOfTribute.Board.Cards;

namespace TalesOfTribute
{
    public class Treasury : Patron
    {
        public override (PlayResult, IEnumerable<CompletedAction>) PatronActivation(Player activator, Player enemy)
        {
            if (!CanPatronBeActivated(activator, enemy))
            {
                return (new Failure("Not enough Coin to activate Treasury"), new List<CompletedAction>());
            }

            activator.CoinsAmount -= 2;
            var inPlayCards = activator.Played.Concat(activator.Hand).Where(c => c.CommonId != CardId.UNKNOWN).ToList();

            return (new Choice(inPlayCards,
                    ChoiceFollowUp.COMPLETE_TREASURY,
                new ChoiceContext(PatronID), 1, 1),
                    new List<CompletedAction>
                    {
                        new(activator.ID, CompletedActionType.GAIN_COIN, PatronID, -2),
                    });
        }

        public override void PatronPower(Player activator, Player enemy)
        {
            // No benefits
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
            List<UniqueCard> inPlayCards = activator.Played.Concat(activator.Hand).Where(c => c.CommonId != CardId.UNKNOWN).ToList();
            return activator.CoinsAmount >= 2 && inPlayCards.Any();
        }
    }
}
