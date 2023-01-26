using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace SimpleBotsTests;

public class RandomBotWithRandomStateExploring : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        for (int i = 0; i < 3; i++)
        {
            var (newState1, newMoves1) = gameState.ApplyState(possibleMoves.PickRandom(Rng));
            var (newState2, newMoves2) = gameState.ApplyState(possibleMoves.PickRandom(Rng));
            
            if (newMoves2.Count > 0)
                (newState2, newMoves2) = newState2.ApplyState(newMoves2.PickRandom(Rng));
            if (newMoves1.Count > 0)
                (newState1, newMoves1) = newState1.ApplyState(newMoves1.PickRandom(Rng));
            if (newMoves1.Count > 0)
                (newState1, newMoves1) = newState1.ApplyState(newMoves1.PickRandom(Rng));
            if (newMoves2.Count > 0)
                (newState2, newMoves2) = newState2.ApplyState(newMoves2.PickRandom(Rng));
        }

        return possibleMoves.PickRandom(Rng);
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}
