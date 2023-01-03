using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;

namespace SimpleBotsTests;

public class RandomMaximizePrestigeBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom();

    public override Move Play(SerializedBoard serializedBoard, List<Move> possibleMoves)
    {
        var movesToCheck = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
        if (movesToCheck.Count == 0)
        {
            return Move.EndTurn();
        }

        Dictionary<int, List<Move>> prestigeToMove = new();
        foreach (var move in movesToCheck)
        {
            var (newState, newPossibleMoves) = serializedBoard.ApplyState(move);
            if (newState.GameEndState?.Winner == Id)
            {
                return move;
            }

            var newMovesToCheck = newPossibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
            if (newMovesToCheck.Count == 0)
                continue;

            foreach (var newMove in newMovesToCheck)
            {
                var (newestState, _) = newState.ApplyState(newMove);
                if (newestState.GameEndState?.Winner == Id)
                {
                    return newMove;
                }

                var val = newestState.CurrentPlayer.Prestige + newestState.CurrentPlayer.Power;
                if (prestigeToMove.ContainsKey(val))
                {
                    prestigeToMove[val].Add(move);
                }
                else
                {
                    prestigeToMove.Add(val, new List<Move> { move });
                }
            }
        }

        if (prestigeToMove.Keys.Count == 0)
        {
            return possibleMoves.PickRandom();
        }

        return prestigeToMove[prestigeToMove.Keys.Max()].PickRandom();
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
