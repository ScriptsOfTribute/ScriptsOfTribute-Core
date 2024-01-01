using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace Bots;

public class TurnTimeoutBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        return availablePatrons[0];
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
    {
        Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        var movesWithoutEndTurn = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();

        if (movesWithoutEndTurn.Any())
        {
            return movesWithoutEndTurn.First();
        }
        return possibleMoves[0];
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}
