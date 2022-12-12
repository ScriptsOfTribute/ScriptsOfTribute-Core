namespace TalesOfTribute
{
    public enum CommandEnum
    {
        PLAY_CARD = 0,
        ATTACK = 1,
        BUY_CARD = 2,
        END_TURN = 3,
        CALL_PATRON = 4,
        MAKE_CHOICE = 5,
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
        
        public static Move MakeChoice(List<EffectType> effects)
        {
            return new MakeChoiceMove<EffectType>(CommandEnum.MAKE_CHOICE, effects);
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
            return Command.GetHashCode() ^ Card.GetHashCode();
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
            return Command.GetHashCode() ^ PatronId.GetHashCode();
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

            return Command == m.Command && Choices == m.Choices;
        }

        public override int GetHashCode()
        {
            return Command.GetHashCode() ^ Choices.GetHashCode();
        }
    }
}
