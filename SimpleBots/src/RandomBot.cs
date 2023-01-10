using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace SimpleBots;

public class RandomBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom();

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        return possibleMoves.PickRandom();
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
