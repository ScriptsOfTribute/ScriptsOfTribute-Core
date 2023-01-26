/*

using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace SimpleBotsTests;

public class RandomMaximizePrestigeBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        var movesToCheck = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
        if (movesToCheck.Count == 0)
        {
            Log(Move.EndTurn().ToString());
            return Move.EndTurn();
        }

        Dictionary<int, List<Move>> prestigeToMove = new();
        foreach (var move in movesToCheck)
        {
            var (newState, newPossibleMoves) = gameState.ApplyState(move, Seed);
            if (newState.GameEndState?.Winner == Id)
            {
                Log(move.ToString());
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
                    Log(move.ToString());
                    return move;
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
            var finalMove = possibleMoves.PickRandom(Rng);
            return finalMove;
        }

        var bestMove = prestigeToMove[prestigeToMove.Keys.Max()].PickRandom(Rng);
        return bestMove;
    }

    public override void GameEnd(EndGameState state){}
}
*/
using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace SimpleBotsTests;

public class RandomMaximizePrestigeBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        var movesToCheck = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
        if (movesToCheck.Count == 0)
        {
            return Move.EndTurn();
        }

        Dictionary<int, List<Move>> prestigeToMove = new();
        foreach (var move in movesToCheck)
        {
            var (newState, newPossibleMoves) = gameState.ApplyState(move);
            if (newState.GameEndState?.Winner == Id)
            {
                return move;
            }

            var newMovesToCheck = newPossibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
            if (newMovesToCheck.Count == 0){
                var val = newState.CurrentPlayer.Prestige + newState.CurrentPlayer.Power;
                if (prestigeToMove.ContainsKey(val)){
                    prestigeToMove[val].Add(move);
                }
                else{
                    prestigeToMove.Add(val, new List<Move> { move });
                }
                continue;
            }

            foreach (var newMove in newMovesToCheck)
            {
                var (newestState, _) = newState.ApplyState(newMove);
                if (newestState.GameEndState?.Winner == Id)
                {
                    return move;
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
            return possibleMoves.PickRandom(Rng);
        }

        var bestMove = prestigeToMove[prestigeToMove.Keys.Max()].PickRandom(Rng);
        Log(bestMove.ToString());
        return bestMove;
    }

    public override void GameEnd(EndGameState state)
    {
        Log("Game ended : (");
    }
}