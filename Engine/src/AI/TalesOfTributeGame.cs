using System.Diagnostics;
using TalesOfTribute.Board;
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
    private int _moveCounter;

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
        var startOfTurnChoices = _api.HandleStartOfTurnChoices();

        if (startOfTurnChoices is null) return null;

        foreach (var choice in startOfTurnChoices.Consume())
        {
            // This can be Success in case there are no more cards to discard.
            if (choice is Success)
            {
                continue;
            }

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

        var result = choice.Choose(makeChoiceMove.Choices);

        if (result is Failure f)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, f.Reason);
        }

        if (result is not Success)
        {
            throw new Exception(
                "There is something wrong in the engine! In case other start of turn choices were added (other than DESTROY), this needs updating - start of turn choices shouldn't return choices!");
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
        => await ConsumeChain(_api.ActivateAgent(move.Card));

    private async Task<EndGameState?> HandlePlayCard(SimpleCardMove move)
        => await ConsumeChain(_api.PlayCard(move.Card));

    private async Task<EndGameState?> HandleBuyCard(SimpleCardMove move)
        => await ConsumeChain(_api.BuyCard(move.Card));

    private async Task<EndGameState?> ConsumeChain(ExecutionChain chain)
    {
        foreach (var result in chain.Consume())
        {
            var endGameState = await HandleTopLevelResult(result);

            if (endGameState is not null)
            {
                return endGameState;
            }
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

        var result = _api.PatronActivation(move.PatronId);

        return result switch
        {
            Failure f => new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, f.Reason),
            BaseChoice => await HandleChoice(result),
            _ => null
        };
    }

    private async Task<EndGameState?> HandleTopLevelResult(PlayResult result)
    {
        return result switch
        {
            Success => null,
            Failure failure => new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, failure.Reason),
            _ => await HandleChoice(result)
        };
    }

    private async Task<EndGameState?> HandleChoice(PlayResult result)
    {
        do
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
                    result = choice.Choose(c.Choices);
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
                    result = choice.Choose(c.Choices);
                    break;
                }
                case Failure failure:
                {
                    return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, failure.Reason);
                }
            }
        }
        while (result is not Success);

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
