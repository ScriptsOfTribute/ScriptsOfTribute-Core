using System.Diagnostics;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute.AI;

public class ScriptsOfTributeGame
{
    public const int TurnLimit = 500;

    private IScriptsOfTributeApi _api;
    private AI[] _players = new AI[2];
    private TimeSpan _currentTurnTimeElapsed = TimeSpan.Zero;
    public EndGameState? EndGameState { get; private set; }
    private AI CurrentPlayer => _players[(int)_api.CurrentPlayerId];
    private AI EnemyPlayer => _players[(int)_api.EnemyPlayerId];
    private TimeSpan _timeout;
    private TimeSpan CurrentTurnTimeRemaining => _timeout - _currentTurnTimeElapsed;
    private List<Move> _moveHistory = new();

    public ScriptsOfTributeGame(AI[] players, IScriptsOfTributeApi api, TimeSpan timeout)
    {
        _api = api;
        _timeout = timeout;
        _players[0] = players[0];
        _players[1] = players[1];
    }

    private Task<Move> MoveTask(GameState state, List<Move> moves)
    {
        return Task.Run(() =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = CurrentPlayer.Play(state, moves, CurrentTurnTimeRemaining);
            stopwatch.Stop();
            _currentTurnTimeElapsed += stopwatch.Elapsed;
            return result;
        });
    }

    private (EndGameState?, Move?) PlayWithTimeout()
    {
        var board = _api.GetFullGameState();
        var state = new GameState(board);
        var moves = _api.GetListOfPossibleMoves();

        var task = MoveTask(state, moves);

        try
        {
            if (task.Wait(CurrentTurnTimeRemaining))
            {
                var result = task.Result;
                _moveHistory.Add(result);
                _api.Logger.Log(CurrentPlayer.LogMessages, CurrentPlayer.Id, _api.TurnCount, _api.TurnMoveCount);
                CurrentPlayer.LogMessages.Clear();
                return (null, result);
            }
        }
        catch (AggregateException e)
        {
            _api.Logger.Log(CurrentPlayer.LogMessages, CurrentPlayer.Id, _api.TurnCount, _api.TurnMoveCount);
            CurrentPlayer.LogMessages.Clear();

            if (e.InnerExceptions.Any())
            {
                var message = string.Join('\n', e.InnerExceptions.Select(e => $"{e.Message}\n{e.StackTrace}\n\n"));
                return (new EndGameState(_api.EnemyPlayerId, GameEndReason.BOT_EXCEPTION, message), null);
            }
            
            return (new EndGameState(PlayerEnum.NO_PLAYER_SELECTED, GameEndReason.INTERNAL_ERROR, $"{e.Message}\n{e.StackTrace}"), null);
        }

        return (new EndGameState(_api.EnemyPlayerId, GameEndReason.TURN_TIMEOUT), null);
    }
    
    public (EndGameState, FullGameState) Play()
    {
        CurrentPlayer.LogMessages.ForEach(m => _api.Logger.Log(CurrentPlayer.Id, m.Item2));
        EnemyPlayer.LogMessages.ForEach(m => _api.Logger.Log(EnemyPlayer.Id, m.Item2));
        CurrentPlayer.LogMessages.Clear();
        EnemyPlayer.LogMessages.Clear();

        EndGameState? endGameState = null;
        do
        {
            var startOfTurnResult = HandleStartOfTurnChoices();
            if (startOfTurnResult is not null)
            {
                return EndGame(startOfTurnResult);
            }

            Move? move;
            do
            {
                (var timeout, move) = PlayWithTimeout();

                if (timeout is not null)
                {
                    return EndGame(timeout);
                }

                if (move is null)
                {
                    throw new EngineException("This shouldn't happen - there is a bug in the engine!");
                }

                var result = HandleFreeMove(move);
                if (result is not null)
                {
                    return EndGame(result);
                }
            } while (move.Command != CommandEnum.END_TURN);

            endGameState = HandleEndTurn();
            if (endGameState is not null)
            {
                return EndGame(endGameState);
            }
        } while ((endGameState = _api.EndTurn()) is null);

        return EndGame(endGameState);
    }

    private EndGameState? HandleEndTurn()
    {
        _api.Logger.Log(CurrentPlayer.LogMessages, CurrentPlayer.Id, _api.TurnCount, _api.TurnMoveCount);
        CurrentPlayer.LogMessages.Clear();
        _currentTurnTimeElapsed = TimeSpan.Zero;
        if (_api.TurnCount > TurnLimit)
        {
            return new EndGameState(PlayerEnum.NO_PLAYER_SELECTED, GameEndReason.TURN_LIMIT_EXCEEDED);
        }

        return null;
    }


    private EndGameState? HandleStartOfTurnChoices()
    {
        if (_api.BoardState != BoardState.START_OF_TURN_CHOICE_PENDING)
        {
            return null;
        }

        SerializedChoice? choice = null;
        while ((choice = _api.PendingChoice) is not null)
        {
            if (choice.Type != Choice.DataType.CARD)
            {
                throw new EngineException(
                    "There is something wrong in the engine! In case other start of turn choices were added (other than DESTROY), this needs updating.");
            }
                
            var result = HandleStartOfTurnChoice();

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private EndGameState? HandleStartOfTurnChoice()
    {
        var (timeout, playersChoice) = PlayWithTimeout();

        if (timeout is not null)
        {
            return timeout;
        }

        if (playersChoice is not MakeChoiceMove<UniqueCard> makeChoiceMove)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Start of turn choice for now is always DESTROY, so should be of type Card.");
        }

        return _api.MakeChoice(makeChoiceMove.Choices);
    }

    private EndGameState? HandleFreeMove(Move move)
    {
        if (!_api.IsMoveLegal(move))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, $"Illegal move - {move}.\nShould be one of:\n{string.Join('\n', _api.GetListOfPossibleMoves().Select(m => m.ToString()))}");
        }

        // This should probably be handled above (this move is not in legal moves), but you can never be to careful...
        if (move.Command == CommandEnum.MAKE_CHOICE)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "You don't have a pending choice.");
        }

        return move.Command switch
        {
            CommandEnum.PLAY_CARD => HandlePlayCard((SimpleCardMove)move),
            CommandEnum.ATTACK => HandleAttack((SimpleCardMove)move),
            CommandEnum.BUY_CARD => HandleBuyCard((SimpleCardMove)move),
            CommandEnum.CALL_PATRON => HandleCallPatron((SimplePatronMove)move),
            CommandEnum.ACTIVATE_AGENT => HandleActivateAgent((SimpleCardMove)move),
            CommandEnum.END_TURN => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private EndGameState? HandleActivateAgent(SimpleCardMove move)
    {
        return _api.ActivateAgent(move.Card) ?? ConsumePotentialPendingMoves();
    }

    private EndGameState? HandlePlayCard(SimpleCardMove move)
    {
        return _api.PlayCard(move.Card) ?? ConsumePotentialPendingMoves();
    }

    private EndGameState? HandleBuyCard(SimpleCardMove move)
    {
        return _api.BuyCard(move.Card) ?? ConsumePotentialPendingMoves();
    }

    private EndGameState? ConsumePotentialPendingMoves()
    {
        SerializedChoice? choice = null;
        try
        {
            while ((choice = _api.PendingChoice) is not null)
            {
                var result = HandleChoice(choice);

                if (result is not null)
                {
                    return result;
                }
            }
        }
        catch (Exception e)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, $"{e.Message}\n{e.StackTrace}\n\n{e.Source}\n\n\n\n\n\n\n\n{e.ToString()}\n");
        }

        return null;
    }

    private EndGameState? HandleAttack(SimpleCardMove move)
        => _api.AttackAgent(move.Card);

    private EndGameState? HandleCallPatron(SimplePatronMove move)
    {
        if (!Enum.IsDefined(typeof(PatronId), move.PatronId))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Invalid patron selected.");
        }

        return _api.PatronActivation(move.PatronId) ?? ConsumePotentialPendingMoves();
    }

    private EndGameState? HandleChoice(SerializedChoice result)
    {
        switch (result.Type)
        {
            case Choice.DataType.CARD:
            {
                var (timeout, move) = PlayWithTimeout();
                if (timeout is not null)
                {
                    return timeout;
                }

                if (move is not MakeChoiceMove<UniqueCard> c)
                {
                    return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE,
                        "Choice of Card was required.");
                }

                var potentialEndState = _api.MakeChoice(c.Choices);
                if (potentialEndState is not null)
                {
                    return potentialEndState;
                }

                break;
            }
            case Choice.DataType.EFFECT:
            {
                var (timeout, move) = PlayWithTimeout();
                if (timeout is not null)
                {
                    return timeout;
                }

                if (move is not MakeChoiceMove<UniqueEffect> c)
                {
                    return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE,
                        "Choice of Effect was required.");
                }
                var potentialEndState = _api.MakeChoice(c.Choices.First());
                if (potentialEndState is not null)
                {
                    return potentialEndState;
                }

                break;
            }
        }

        return null;
    }

    private (EndGameState, FullGameState) EndGame(EndGameState state)
    {
        state.AdditionalContext +=
            $"\nLast few moves for context:\n{string.Join('\n', _moveHistory.TakeLast(5).Select(m => m.ToString()))}";
        CurrentPlayer.GameEnd(state, _api.GetFullGameState());
        CurrentPlayer.LogMessages.ForEach(m => _api.Logger.Log(CurrentPlayer.Id, m.Item2));
        EnemyPlayer.GameEnd(state, _api.GetFullGameState());
        EnemyPlayer.LogMessages.ForEach(m => _api.Logger.Log(EnemyPlayer.Id, m.Item2));
        EndGameState = state;
        _api.Logger.Flush();
        return (state, _api.GetFullGameState());
    }
}
