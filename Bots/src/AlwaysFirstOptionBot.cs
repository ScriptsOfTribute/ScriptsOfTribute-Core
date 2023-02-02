using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace SimpleBots;

public class AlwaysFirstOptionBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        return availablePatrons[0];
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        return possibleMoves[0];
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}
