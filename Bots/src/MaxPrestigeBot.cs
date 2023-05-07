using Bots;
using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace Bots;

public class MaxPrestigeBot : AI
{
    private string patronLogPath = "patronsMaxPrestigeBot.txt";
    private Apriori apriori = new Apriori();
    private int support = 4;
    private double confidence = 0.3;
    private PlayerEnum myID;
    private string patrons;
    private bool startOfGame = true;
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        //PatronId? selectedPatron = apriori.AprioriBestChoice(availablePatrons, patronLogPath, support, confidence);
        //return selectedPatron ?? availablePatrons.PickRandom(Rng);
        => availablePatrons.PickRandom(Rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfGame)
        {
            myID = gameState.CurrentPlayer.PlayerID;
            patrons = string.Join(",", gameState.Patrons.FindAll(x => x != PatronId.TREASURY).Select(n => n.ToString()).ToArray());
            startOfGame = false;
        }

        var movesToCheck = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
        if (movesToCheck.Count == 0)
        {
            Log(Move.EndTurn().ToString());
            return Move.EndTurn();
        }

        Dictionary<int, List<Move>> prestigeToMove = new();
        foreach (var move in movesToCheck)
        {
            var (newState, newPossibleMoves) = gameState.ApplyMove(move, Seed);
            if (newState.GameEndState?.Winner == Id)
            {
                Log(move.ToString());
                return move;
            }

            var newMovesToCheck = newPossibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
            if (newMovesToCheck.Count == 0)
            {
                var val = newState.CurrentPlayer.Prestige + newState.CurrentPlayer.Power;
                if (prestigeToMove.ContainsKey(val))
                {
                    prestigeToMove[val].Add(move);
                }
                else
                {
                    prestigeToMove.Add(val, new List<Move> { move });
                }
                continue;
            }

            foreach (var newMove in newMovesToCheck)
            {
                var (newestState, _) = newState.ApplyMove(newMove);
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
            Log(finalMove.ToString());
            return finalMove;
        }

        var bestMove = prestigeToMove[prestigeToMove.Keys.Max()].PickRandom(Rng);
        Log(bestMove.ToString());
        return bestMove;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        Log("Game ended : (");
        if (state.Winner == myID)
        {
            File.AppendAllText(patronLogPath, patrons + System.Environment.NewLine);
        }
        startOfGame = true;
    }
}