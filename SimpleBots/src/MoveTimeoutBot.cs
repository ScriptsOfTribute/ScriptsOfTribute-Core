using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;

namespace SimpleBots;

public class MoveTimeoutBot : AI
{
    public override TimeSpan MoveTimeout { get; } = TimeSpan.FromMicroseconds(1);

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        return availablePatrons[0];
    }

    public override Move Play(SerializedBoard serializedBoard, List<Move> possibleMoves)
    {
        long ctr = 0;
        for (var i = 0; i < int.MaxValue; i++)
        {
            ctr += i;
        }

        return possibleMoves[0];
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
