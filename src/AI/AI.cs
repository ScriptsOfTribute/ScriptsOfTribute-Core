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
        public abstract PatronId SelectPatron(List<PatronId> availablePatrons, int round); // Will be called only twice
        public abstract Move Play(SerializedBoard serializedBoard);
        public abstract List<Effect> HandleEffectChoice(SerializedBoard serializedBoard, SerializedChoice<Effect> choice);
        public abstract List<Card> HandleCardChoice(SerializedBoard serializedBoard, SerializedChoice<Card> choice);
        public abstract List<Card> HandleStartOfTurnChoice(SerializedBoard serializedBoard, SerializedChoice<Card> choice);

        // TODO: Placeholder for now, will require more args
        public abstract void HandleFailure(string reason);
    }
}
