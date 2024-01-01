using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace Bots;

public class PatronFavorsBot : AI
{
    private readonly SeededRandom rng = new(123);
    
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(rng);

    private Move? ActivatePatronWhichDoesntFavorMe(SeededGameState gameState, List<Move> movesWithoutEndTurn)
    {
        Dictionary<PatronId, PlayerEnum> patronFavours = gameState.PatronStates.All;

        foreach (Move m in movesWithoutEndTurn)
        {
            if (m.Command == CommandEnum.CALL_PATRON)
            {
                SimplePatronMove move = m as SimplePatronMove;
                if (patronFavours[move.PatronId] != gameState.CurrentPlayer.PlayerID && move.PatronId != PatronId.TREASURY)
                {
                    return m;
                }
            }
        }
        return null;
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
    {
        var movesWithoutEndTurn = possibleMoves.Where(move => move.Command != CommandEnum.END_TURN).ToList();
        if (movesWithoutEndTurn.Count == 0)
        {
            return Move.EndTurn();
        }

        var seededGameState = gameState.ToSeededGameState(123);
        Move? patronMove = ActivatePatronWhichDoesntFavorMe(seededGameState, movesWithoutEndTurn);
        if (patronMove is not null)
        {
            return patronMove;
        }

        foreach (Move m in movesWithoutEndTurn)
        {
            if (m.Command == CommandEnum.BUY_CARD)
            {
                SimpleCardMove move = m as SimpleCardMove;
                if (move.Card.Name == "Tithe" && gameState.CurrentPlayer.PatronCalls <= 0)
                {
                    (var newGameState, List<Move> newPossibleMoves) = seededGameState.ApplyMove(m);
                    if (ActivatePatronWhichDoesntFavorMe(newGameState, newPossibleMoves) is not null)
                    {
                        return m;
                    }
                }
            }
        }

        return movesWithoutEndTurn.PickRandom(rng);
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}
