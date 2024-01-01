using Bots;
using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace Bots;

public class RandomBotWithRandomStateExploring : AI
{
    private readonly SeededRandom rng = new(123);

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
    {
        for (int i = 0; i < 3; i++)
        {
            var (newState1, newMoves1) = gameState.ApplyMove(possibleMoves.PickRandom(rng), 123);
            var (newState2, newMoves2) = gameState.ApplyMove(possibleMoves.PickRandom(rng), 123);

            if (newMoves2.Count > 0)
                (newState2, newMoves2) = newState2.ApplyMove(newMoves2.PickRandom(rng));
            if (newMoves1.Count > 0)
                (newState1, newMoves1) = newState1.ApplyMove(newMoves1.PickRandom(rng));
            if (newMoves1.Count > 0)
                (newState1, newMoves1) = newState1.ApplyMove(newMoves1.PickRandom(rng));
            if (newMoves2.Count > 0)
                (newState2, newMoves2) = newState2.ApplyMove(newMoves2.PickRandom(rng));
        }

        return possibleMoves.PickRandom(rng);
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}
