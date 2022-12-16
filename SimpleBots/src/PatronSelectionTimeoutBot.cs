using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;

namespace SimpleBots;

public class PatronSelectionTimeoutBot : AI
{
    public override TimeSpan MoveTimeout { get; } = TimeSpan.FromSeconds(1);

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        Task.Delay(TimeSpan.FromSeconds(2)).Wait();
        return availablePatrons[0];
    }

    public override Move Play(SerializedBoard serializedBoard, List<Move> possibleMoves)
    {
        throw new NotImplementedException();
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
