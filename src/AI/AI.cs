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

        public abstract PatronId SelectPatron(List<PatronId> avalaiblePatrons); // Will be called only twice
        public abstract Move Play(BoardSerializer board);
        public abstract List<Effect> HandleEffectChoice(BoardSerializer board, Choice<Effect> choice);
        public abstract List<Card> HandleCardChoice(BoardSerializer board, Choice<Card> choice);

        public abstract
    }
}
