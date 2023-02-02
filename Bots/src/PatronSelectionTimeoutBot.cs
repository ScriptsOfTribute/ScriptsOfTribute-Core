using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace SimpleBots;

public class PatronSelectionTimeoutBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        Task.Delay(TimeSpan.FromSeconds(3)).Wait();
        return availablePatrons[0];
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        throw new NotImplementedException();
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}
