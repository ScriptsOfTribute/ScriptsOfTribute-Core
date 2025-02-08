using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute
{
    public enum CommandEnum
    {
        PLAY_CARD,
        ACTIVATE_AGENT,
        ATTACK,
        BUY_CARD,
        CALL_PATRON,
        MAKE_CHOICE,
        END_TURN,
    }

    public class Move
    {
        public CommandEnum Command { get; }
        public UniqueId UniqueId { get; }

        protected Move(CommandEnum command)
        {
            Command = command;
            UniqueId = UniqueId.Create();
        }

        public static Move PlayCard(UniqueCard card)
        {
            return new SimpleCardMove(CommandEnum.PLAY_CARD, card);
        }

        public static Move ActivateAgent(UniqueCard card)
        {
            return new SimpleCardMove(CommandEnum.ACTIVATE_AGENT, card);
        }

        public static Move Attack(UniqueCard card)
        {
            return new SimpleCardMove(CommandEnum.ATTACK, card);
        }

        public static Move BuyCard(UniqueCard card)
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

        public static Move MakeChoice(UniqueCard card)
        {
            return new MakeChoiceMove<UniqueCard>(CommandEnum.MAKE_CHOICE, new List<UniqueCard> { card });
        }

        public static Move MakeChoice(List<UniqueCard> cards)
        {
            return new MakeChoiceMoveUniqueCard(CommandEnum.MAKE_CHOICE, cards);
        }

        public static Move MakeChoice(UniqueEffect effect)
        {
            return new MakeChoiceMoveUniqueEffect(CommandEnum.MAKE_CHOICE, new List<UniqueEffect> { effect });
        }

        public static Move MakeChoice(List<UniqueEffect> effects)
        {
            return new MakeChoiceMoveUniqueEffect(CommandEnum.MAKE_CHOICE, effects);
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
            return $"{Command}";
        }

        public virtual string ToJSON() {
            if (Command == CommandEnum.END_TURN)
            {
                return $"{Command}";
            }
            else
            {
                return $"NotImplementedYet {Command}";
            }
        }

    }

    public class SimpleCardMove : Move
    {
        public readonly UniqueCard Card;

        internal SimpleCardMove(CommandEnum command, UniqueCard card) : base(command)
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
            return $"{Command} {Card.CommonId}";
        }

        public override string ToJSON()
        {
            return $"{Command} {Card.UniqueId}";
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
            return $"PATRON {PatronId}";
        }

        public override string ToJSON()
        {
            return $"PATRON {PatronId}";
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
            if (Choices != null && Choices.Count > 0 && Choices[0].GetType() == typeof(UniqueCard))
                return $"CHOICE {string.Join(' ', Choices.Select(c => (c != null) ? ((UniqueCard)(object)c).Name.ToString().ToUpper().Replace(" ", "_") : c?.GetType().ToString())) }";

            if (Choices != null && Choices.Count > 0 && Choices[0].GetType() == typeof(UniqueEffect))
                return $"CHOICE {string.Join(' ', Choices.Select(c => (c != null) ? ((UniqueEffect)(object)c).ParentCard.Name.ToString() : c?.GetType().ToString()))}";


            return $"CHOICE {String.Join(" ", Choices.Select(c => c.ToString() + " " + c.GetType().ToString()).ToList())}";
        }

        public override string ToJSON()
        {
            var output = new List<string>();
            bool effectHandled = false;

            Type listType = Choices.GetType();
            Type[] genericArguments = listType.GetGenericArguments();

            foreach (T choice in Choices)
            {
                if (genericArguments[0] == typeof(UniqueEffect))
                {
                    output.Add("LEFT");
                    output.Add("RIGHT");
                    effectHandled = true;
                    continue;
                }
                else if (choice is UniqueCard card)
                {
                    output.Add(card.Name.ToString());
                }
            }
            if (output.Count > 0)
                return $"CHOICE {string.Join(' ', output)}";
            return "CHOICE";
        }
    }

    public class MakeChoiceMoveUniqueCard : MakeChoiceMove<UniqueCard>
    {
        internal MakeChoiceMoveUniqueCard(CommandEnum command, List<UniqueCard> choices) : base(command, choices)
        {
        }

        public override string ToJSON()
        {
            return $"CHOICE {string.Join(' ', Choices.Select(c => c?.Name.ToUpper().Replace(" ", "_").ToString()))}";
        }
    }

    public class MakeChoiceMoveUniqueEffect : MakeChoiceMove<UniqueEffect>
    {

        internal MakeChoiceMoveUniqueEffect(CommandEnum command, List<UniqueEffect> choices) : base(command, choices)
        {
        }

        public override string ToJSON()
        {
            return $"CHOICE {string.Join(' ', Choices.Select(c => c?.ParentCard.Name.ToUpper().Replace(" ", "_").ToString()))}";
        }
    }
}
