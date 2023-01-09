using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace SimpleBots;

public class MoveTimeoutBot : AI
{
    public override TimeSpan MoveTimeout { get; } = TimeSpan.FromSeconds(3);

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        return availablePatrons[0];
    }

    public override Move Play(GameState serializedBoard, List<Move> possibleMoves)
    {
        Task.Delay(TimeSpan.FromSeconds(4)).Wait();
        return possibleMoves[0];
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
