namespace TalesOfTribute
{
    class Parser
    {

        public Move FromStringToMove(string move)
        {
            string[] splittedMove = move.Split(' ');

            if (splittedMove.Length != 2)
            {
                throw new InvalidOperationException();
            }
            try
            {
                int value = Int32.Parse(splittedMove[1]);
                return new Move(splittedMove[0], value);
            }
            catch (FormatException e)
            {
                throw new InvalidOperationException();
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException();
            }
        }

        public void Parser(string move)
        {
            Move playerMove = FromStringToMove(move);

            switch (playerMove.Command)
            {
                case CommandEnum.GET_POSSIBLE_MOVES:
                    List<Move> moves = GetListOfPossibleMoves();
                    foreach (var move in moves)
                    {
                        Console.WriteLine(move.ToString());
                    }
                    break;

                case CommandEnum.END_TURN:
                    _boardManager.EndTurn();
                    break;
                //TODO rest
                default:
                    break;
            }
        }
    }
}