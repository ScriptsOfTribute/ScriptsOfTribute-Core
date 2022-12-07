using System;
using System.Collections.Generic;
using System.Text;

namespace TalesOfTribute.src.AI
{
    public abstract class AI
    {
        public PlayerEnum PlayerID { get; set; }

        public AI()
        {
        }

        // Round - which selection this is (first or second)
        public abstract PatronId SelectPatron(List<PatronId> avalaiblePatrons, int round); // Will be called only twice
        public abstract Move Play(BoardSerializer board);
        public abstract List<Effect> HandleEffectChoice(BoardSerializer board, SerializedChoice<Effect> choice);
        public abstract List<Card> HandleCardChoice(BoardSerializer board, SerializedChoice<Card> choice);
        public abstract List<Card> HandleStartOfTurnChoice(BoardSerializer board, SerializedChoice<Card> choice);

        // TODO: Placeholder for now, will require more args
        public abstract void HandleChoiceFailure(string reason);
    }
}
