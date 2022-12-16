using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.AI;

public abstract class AI
{
    public virtual TimeSpan TurnTimeout { get; } = TimeSpan.FromSeconds(10);

    public virtual TimeSpan MoveTimeout { get; } = TimeSpan.FromSeconds(3);

    // Round - which selection this is (first or second)
    public abstract PatronId SelectPatron(List<PatronId> availablePatrons, int round); // Will be called only twice
    public abstract Move Play(SerializedBoard serializedBoard, List<Move> possibleMoves);
    public abstract void GameEnd(EndGameState state);
}
