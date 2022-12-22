using System.Diagnostics;
using TalesOfTribute.Board;
using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.AI;

public class TalesOfTributeGame
{
    public const int TurnLimit = 500;

    private ITalesOfTributeApi _api;
    private AI[] _players = new AI[2];
    private TimeSpan _currentTurnTimeElapsed = TimeSpan.Zero;
    public EndGameState? EndGameState { get; private set; }
    private AI CurrentPlayer => _players[(int)_api.CurrentPlayerId];
    private AI EnemyPlayer => _players[(int)_api.EnemyPlayerId];
    private TimeSpan CurrentTurnTimeRemaining => CurrentPlayer.TurnTimeout - _currentTurnTimeElapsed;

    public TalesOfTributeGame(AI[] players, ITalesOfTributeApi api)
    {
        _api = api;
        _players[0] = players[0];
        _players[1] = players[1];
    }

    private async Task<Move> MoveTask(SerializedBoard board, List<Move> moves)
    {
        return await Task.Run(() =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = CurrentPlayer.Play(board, moves);
            stopwatch.Stop();
            _currentTurnTimeElapsed += stopwatch.Elapsed;
            return result;
        });
    }

    private async Task<(EndGameState?, Move?)> PlayWithTimeout()
    {
        TimeSpan timeout;
        GameEndReason timeoutType;
        if (CurrentPlayer.MoveTimeout < CurrentTurnTimeRemaining)
        {
            timeout = CurrentPlayer.MoveTimeout;
            timeoutType = GameEndReason.MOVE_TIMEOUT;
        }
        else
        {
            timeout = CurrentTurnTimeRemaining;
            timeoutType = GameEndReason.TURN_TIMEOUT;
        }
        var board = _api.GetSerializer();
        var moves = _api.GetListOfPossibleMoves();

        var task = MoveTask(board, moves);
        var res = await Task.WhenAny(task, Task.Delay(timeout));
        
        if (res == task)
        {

            return (null, task.Result);
        }

        return (new EndGameState(_api.EnemyPlayerId, timeoutType), null);
    }
    
    public async Task<EndGameState> Play()
    {
        EndGameState? endGameState;
        while ((endGameState = _api.CheckWinner()) is null)
        {
            var startOfTurnResult = await HandleStartOfTurnChoices();
            if (startOfTurnResult is not null)
            {
                return EndGame(startOfTurnResult);
            }

            Move? move;
            do
            {
                (var timeout, move) = await PlayWithTimeout();

                if (timeout is not null)
                {
                    return timeout;
                }

                if (move is null)
                {
                    throw new Exception("This shouldn't happen - there is a bug in the engine!");
                }

                var result = await HandleFreeMove(move);
                if (result is not null)
                {
                    return EndGame(result);
                }
            } while (move.Command != CommandEnum.END_TURN);

            endGameState = HandleEndTurn();
            if (endGameState is not null)
            {
                return endGameState;
            }

            _api.EndTurn();
        }

        return EndGame(endGameState);
    }

    private EndGameState? HandleEndTurn()
    {
        _currentTurnTimeElapsed = TimeSpan.Zero;
        if (_api.TurnCount > TurnLimit)
        {
            return new EndGameState(PlayerEnum.NO_PLAYER_SELECTED, GameEndReason.TURN_LIMIT_EXCEEDED);
        }

        return null;
    }


    private async Task<EndGameState?> HandleStartOfTurnChoices()
    {
        if (_api.BoardState != BoardState.START_OF_TURN_CHOICE_PENDING)
        {
            return null;
        }

        BaseChoice? choice = null;
        while ((choice = _api.PendingChoice) is not null)
        {
            if (choice is not Choice<Card> realChoice)
            {
                throw new Exception(
                    "There is something wrong in the engine! In case other start of turn choices were added (other than DESTROY), this needs updating.");
            }
                
            var result = await HandleStartOfTurnChoice(realChoice);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private async Task<EndGameState?> HandleStartOfTurnChoice(Choice<Card> choice)
    {
        var (timeout, playersChoice) = await PlayWithTimeout();

        if (timeout is not null)
        {
            return timeout;
        }

        if (playersChoice is not MakeChoiceMove<Card> makeChoiceMove)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Start of turn choice for now is always DESTROY, so should be of type Card.");
        }

        try
        {
            _api.MakeChoice(makeChoiceMove.Choices);
        }
        catch (Exception e)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, e.Message);
        }

        return null;
    }

    private async Task<EndGameState?> HandleFreeMove(Move move)
    {
        if (!_api.IsMoveLegal(move))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Illegal move.");
        }

        // This should probably be handled above (this move is not in legal moves), but you can never be to careful...
        if (move.Command == CommandEnum.MAKE_CHOICE)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "You don't have a pending choice.");
        }

        return move.Command switch
        {
            CommandEnum.PLAY_CARD => await HandlePlayCard(move as SimpleCardMove),
            CommandEnum.ATTACK => HandleAttack(move as SimpleCardMove),
            CommandEnum.BUY_CARD => await HandleBuyCard(move as SimpleCardMove),
            CommandEnum.CALL_PATRON => await HandleCallPatron(move as SimplePatronMove),
            CommandEnum.ACTIVATE_AGENT => await HandleActivateAgent(move as SimpleCardMove),
            CommandEnum.END_TURN => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<EndGameState?> HandleActivateAgent(SimpleCardMove move)
    {
        try
        {
            _api.ActivateAgent(move.Card);
        }
        catch (Exception e)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, e.Message);
        }

        return await ConsumePotentialPendingMoves();
    }
    
    private async Task<EndGameState?> HandlePlayCard(SimpleCardMove move)
    {
        try
        {
            _api.PlayCard(move.Card);
        }
        catch (Exception e)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, e.Message);
        }

        return await ConsumePotentialPendingMoves();
    }
    
    private async Task<EndGameState?> HandleBuyCard(SimpleCardMove move)
    {
        try
        {
            _api.BuyCard(move.Card);
        }
        catch (Exception e)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, e.Message);
        }

        return await ConsumePotentialPendingMoves();
    }

    private async Task<EndGameState?> ConsumePotentialPendingMoves()
    {
        BaseChoice? choice = null;
        try
        {
            while ((choice = _api.PendingChoice) is not null)
            {
                var result = await HandleChoice(choice);

                if (result is not null)
                {
                    return result;
                }
            }
        }
        catch (Exception e)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, e.Message);
        }

        return null;
    }

    private EndGameState? HandleAttack(SimpleCardMove move)
    {
        if (_api.AttackAgent(move.Card) is Failure f)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, f.Reason);
        }

        return null;
    }

    private async Task<EndGameState?> HandleCallPatron(SimplePatronMove move)
    {
        if (!Enum.IsDefined(typeof(PatronId), move.PatronId))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Invalid patron selected.");
        }

        _api.PatronActivation(move.PatronId);

        return await ConsumePotentialPendingMoves();
    }

    private async Task<EndGameState?> HandleChoice(BaseChoice result)
    {
        switch (result)
        {
            case Choice<Card> choice:
            {
                var (timeout, move) = await PlayWithTimeout();
                if (timeout is not null)
                {
                    return timeout;
                }

                if (move is not MakeChoiceMove<Card> c)
                {
                    return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE,
                        "Choice of Card was required.");
                }
                
                _api.MakeChoice(c.Choices);
                break;
            }
            case Choice<EffectType> choice:
            {
                var (timeout, move) = await PlayWithTimeout();
                if (timeout is not null)
                {
                    return timeout;
                }

                if (move is not MakeChoiceMove<EffectType> c)
                {
                    return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE,
                        "Choice of Effect was required.");
                }
                _api.MakeChoice(c.Choices);
                break;
            }
        }

        return null;
    }

    private EndGameState EndGame(EndGameState state)
    {
        CurrentPlayer.GameEnd(state);
        EnemyPlayer.GameEnd(state);
        EndGameState = state;
        return state;
    }
}
