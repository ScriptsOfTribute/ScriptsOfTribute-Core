using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;

namespace SimpleBots;

public class RandomBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom();

    public override Move Play(SerializedBoard serializedBoard, List<Move> possibleMoves)
        => possibleMoves.PickRandom();

    public override void GameEnd(EndGameState state)
    {
    }
}
