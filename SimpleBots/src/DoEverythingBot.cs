using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace SimpleBots;

// Bot that plays all available cards before ending turn.
public class DoEverythingBot : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        var movesWithoutEndTurn = possibleMoves.Where(move => move.Command != CommandEnum.END_TURN).ToList();
        if (movesWithoutEndTurn.Count == 0)
        {
            return Move.EndTurn();
        }

        return movesWithoutEndTurn.PickRandom(Rng);
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
