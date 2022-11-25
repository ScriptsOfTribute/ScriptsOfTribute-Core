namespace TalesOfTribute
{
    public enum CommandEnum
    {
        PLAY_CARD = 0,
        ATTACK = 1,
        BUY_CARD = 2,
        END_TURN = 3,
        PATRON = 4,
        GET_POSSIBLE_MOVES = 5,
    }

    public struct Move
    {

        public CommandEnum Command { get; }
        public int Value { get; }

        public Move(string command, int value = -1)
        {
            if (Enum.TryParse(command, out CommandEnum commandConverted))
            {
              Command = commandConverted;
              Value = value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public Move(CommandEnum command, int value = -1)
        {

            Command = command;
            Value = value;
        }

        public override string ToString() => $ "({Command} {Value})";
  }
}