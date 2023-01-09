using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace SimpleBotsTests;

public class RandomBotWithRandomStateExploring : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom();

    public override Move Play(GameState serializedBoard, List<Move> possibleMoves)
    {
        for (int i = 0; i < 3; i++)
        {
            var (newState1, newMoves1) = serializedBoard.ApplyState(possibleMoves.PickRandom());
            var (newState2, newMoves2) = serializedBoard.ApplyState(possibleMoves.PickRandom());
            
            if (newMoves2.Count > 0)
                (newState2, newMoves2) = newState2.ApplyState(newMoves2.PickRandom());
            if (newMoves1.Count > 0)
                (newState1, newMoves1) = newState1.ApplyState(newMoves1.PickRandom());
            if (newMoves1.Count > 0)
                (newState1, newMoves1) = newState1.ApplyState(newMoves1.PickRandom());
            if (newMoves2.Count > 0)
                (newState2, newMoves2) = newState2.ApplyState(newMoves2.PickRandom());
        }

        return possibleMoves.PickRandom();
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
