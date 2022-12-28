namespace TalesOfTribute
{
    public enum CommandEnum
    {
        PLAY_CARD,
        ACTIVATE_AGENT,
        ATTACK,
        BUY_CARD,
        CALL_PATRON,
        MAKE_CHOICE,
        END_TURN ,
    }

    public class Move
    {
        public CommandEnum Command { get; }

        protected Move(CommandEnum command)
        {
            Command = command;
        }

        public static Move PlayCard(Card card)
        {
            return new SimpleCardMove(CommandEnum.PLAY_CARD, card);
        }
        
        public static Move ActivateAgent(Card card)
        {
            return new SimpleCardMove(CommandEnum.ACTIVATE_AGENT, card);
        }
        
        public static Move Attack(Card card)
        {
            return new SimpleCardMove(CommandEnum.ATTACK, card);
        }
        
        public static Move BuyCard(Card card)
        {
            return new SimpleCardMove(CommandEnum.BUY_CARD, card);
        }
        
        public static Move EndTurn()
        {
            return new Move(CommandEnum.END_TURN);
        }

        public static Move CallPatron(PatronId patronId)
        {
            return new SimplePatronMove(CommandEnum.CALL_PATRON, patronId);
        }
        
        public static Move MakeChoice(List<Card> cards)
        {
            return new MakeChoiceMove<Card>(CommandEnum.MAKE_CHOICE, cards);
        }
        
        public static Move MakeChoice(List<Effect> effects)
        {
            return new MakeChoiceMove<Effect>(CommandEnum.MAKE_CHOICE, effects);
        }

        public override bool Equals(object obj)
        {
            if (obj is not Move m)
            {
                return false;
            }

            return Command == m.Command;
        }

        public override int GetHashCode()
        {
            return Command.GetHashCode();
        }

        public override string ToString()
        {
            return $"Move: {Command}";
        }
    }

    public class SimpleCardMove : Move
    {
        public readonly Card Card;

        internal SimpleCardMove(CommandEnum command, Card card) : base(command)
        {
            Card = card;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is not SimpleCardMove m)
            {
                return false;
            }

            return Command == m.Command && Card == m.Card;
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + Command.GetHashCode();
            hash = (hash * 7) + Card.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"Simple card move: {Command} {Card.CommonId}";
        }
    }
    
    public class SimplePatronMove : Move
    {
        public readonly PatronId PatronId;

        internal SimplePatronMove(CommandEnum command, PatronId patronId) : base(command)
        {
            PatronId = patronId;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is not SimplePatronMove m)
            {
                return false;
            }

            return Command == m.Command && PatronId == m.PatronId;
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + Command.GetHashCode();
            hash = (hash * 7) + PatronId.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"Simple Patron Move: {PatronId}";
        }
    }

    public abstract class BaseMakeChoiceMove : Move
    {
        protected BaseMakeChoiceMove(CommandEnum command) : base(command)
        {
        }
    }

    public class MakeChoiceMove<T> : BaseMakeChoiceMove
    {
        public readonly List<T> Choices;

        internal MakeChoiceMove(CommandEnum command, List<T> choices) : base(command)
        {
            Choices = choices.ToList();
        }
        
        public override bool Equals(object obj)
        {
            if (obj is not MakeChoiceMove<T> m)
            {
                return false;
            }

            return Command == m.Command && Choices.SequenceEqual(m.Choices);
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + Command.GetHashCode();
            hash = (hash * 7) + Choices.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"Make choice move: {string.Join(' ', Choices.Select(c => c?.ToString()))}";
        }
    }
}
