using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace SimpleBots;

public class WinByPatronFavors : AI
{
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(Rng);

    private Move? ActivatePatronWhichDoesntFavorMe(GameState gameState, List<Move> movesWithoutEndTurn){
        Dictionary<PatronId, PlayerEnum> patronFavours = gameState.PatronStates.All;

        foreach(Move m in movesWithoutEndTurn){
            if (m.Command == CommandEnum.CALL_PATRON){
                SimplePatronMove move = m as SimplePatronMove;
                if (patronFavours[move.PatronId] != gameState.CurrentPlayer.PlayerID && move.PatronId != PatronId.TREASURY){
                    return m;
                }
            }
        }
        return null;
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        var movesWithoutEndTurn = possibleMoves.Where(move => move.Command != CommandEnum.END_TURN).ToList();
        if (movesWithoutEndTurn.Count == 0)
        {
            return Move.EndTurn();
        }

        Move? patronMove = ActivatePatronWhichDoesntFavorMe(gameState, movesWithoutEndTurn);
        if (patronMove is not null){
            return patronMove;
        }

        foreach (Move m in movesWithoutEndTurn){
            if (m.Command == CommandEnum.BUY_CARD){
                SimpleCardMove move = m as SimpleCardMove;
                if (move.Card.Name == "Tithe" && gameState.CurrentPlayer.PatronCalls <=0){
                    (GameState newGameState, List<Move> newPossibleMoves) = gameState.ApplyState(m);
                    if (ActivatePatronWhichDoesntFavorMe(newGameState, newPossibleMoves) is not null){
                        return m;
                    }
                }
            }
        }

        return movesWithoutEndTurn.PickRandom(Rng);
    }

    public override void GameEnd(EndGameState state)
    {
    }
}
