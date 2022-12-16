using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;

namespace SimpleBots;

public class TurnTimeoutBot : AI
{
    public override TimeSpan MoveTimeout { get; } = TimeSpan.FromSeconds(2);
    public override TimeSpan TurnTimeout { get; } = TimeSpan.FromSeconds(4);

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        return availablePatrons[0];
    }

    public override Move Play(SerializedBoard serializedBoard, List<Move> possibleMoves)
    {
        Task.Delay(TimeSpan.FromSeconds(1.5)).Wait();
        return possibleMoves[0];
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
