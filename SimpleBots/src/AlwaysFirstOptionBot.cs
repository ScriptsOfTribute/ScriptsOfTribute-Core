using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

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

    public override void GameEnd(EndGameState state)
    {
    }
}
