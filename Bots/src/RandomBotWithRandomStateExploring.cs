using Bots;
using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace Bots;

public class RandomBotWithRandomStateExploring : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        for (int i = 0; i < 3; i++)
        {
            var (newState1, newMoves1) = gameState.ApplyMove(possibleMoves.PickRandom(Rng), 123);
            var (newState2, newMoves2) = gameState.ApplyMove(possibleMoves.PickRandom(Rng), 123);

            if (newMoves2.Count > 0)
                (newState2, newMoves2) = newState2.ApplyMove(newMoves2.PickRandom(Rng));
            if (newMoves1.Count > 0)
                (newState1, newMoves1) = newState1.ApplyMove(newMoves1.PickRandom(Rng));
            if (newMoves1.Count > 0)
                (newState1, newMoves1) = newState1.ApplyMove(newMoves1.PickRandom(Rng));
            if (newMoves2.Count > 0)
                (newState2, newMoves2) = newState2.ApplyMove(newMoves2.PickRandom(Rng));
        }

        return possibleMoves.PickRandom(Rng);
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}
