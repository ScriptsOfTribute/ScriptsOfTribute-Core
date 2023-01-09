﻿using System.Diagnostics;
using TalesOfTribute.Board;
using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Board.Cards;
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
    private List<Move> _moveHistory = new();

    public TalesOfTributeGame(AI[] players, ITalesOfTributeApi api)
    {
        _api = api;
        _players[0] = players[0];
        _players[1] = players[1];
    }

    private async Task<Move> MoveTask(GameState state, List<Move> moves)
    {
        return await Task.Run(() =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = CurrentPlayer.Play(state, moves);
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
        var state = new GameState(board);
        var moves = _api.GetListOfPossibleMoves();

        var task = MoveTask(state, moves);
        var res = await Task.WhenAny(task, Task.Delay(timeout));

        if (res == task)
        {
            var result = task.Result;
            _moveHistory.Add(result);
            return (null, result);
        }

        return (new EndGameState(_api.EnemyPlayerId, timeoutType), null);
    }
    
    public async Task<(EndGameState, SerializedBoard)> Play()
    {
        EndGameState? endGameState = null;
        do
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
                    return EndGame(timeout);
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
                return EndGame(endGameState);
            }
        } while ((endGameState = _api.EndTurn()) is null);

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

        SerializedChoice? choice = null;
        while ((choice = _api.PendingChoice) is not null)
        {
            if (choice.Type != Choice.DataType.CARD)
            {
                throw new Exception(
                    "There is something wrong in the engine! In case other start of turn choices were added (other than DESTROY), this needs updating.");
            }
                
            var result = await HandleStartOfTurnChoice();

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private async Task<EndGameState?> HandleStartOfTurnChoice()
    {
        var (timeout, playersChoice) = await PlayWithTimeout();

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

    private async Task<EndGameState?> HandleFreeMove(Move move)
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
            CommandEnum.PLAY_CARD => await HandlePlayCard((SimpleCardMove)move),
            CommandEnum.ATTACK => HandleAttack((SimpleCardMove)move),
            CommandEnum.BUY_CARD => await HandleBuyCard((SimpleCardMove)move),
            CommandEnum.CALL_PATRON => await HandleCallPatron((SimplePatronMove)move),
            CommandEnum.ACTIVATE_AGENT => await HandleActivateAgent((SimpleCardMove)move),
            CommandEnum.END_TURN => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<EndGameState?> HandleActivateAgent(SimpleCardMove move)
    {
        return _api.ActivateAgent(move.Card) ?? await ConsumePotentialPendingMoves();
    }

    private async Task<EndGameState?> HandlePlayCard(SimpleCardMove move)
    {
        return _api.PlayCard(move.Card) ?? await ConsumePotentialPendingMoves();
    }

    private async Task<EndGameState?> HandleBuyCard(SimpleCardMove move)
    {
        return _api.BuyCard(move.Card) ?? await ConsumePotentialPendingMoves();
    }

    private async Task<EndGameState?> ConsumePotentialPendingMoves()
    {
        SerializedChoice? choice = null;
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
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, $"{e.Message}\n{e.StackTrace}\n\n{e.Source}\n\n\n\n\n\n\n\n{e.ToString()}\n");
        }

        return null;
    }

    private EndGameState? HandleAttack(SimpleCardMove move)
        => _api.AttackAgent(move.Card);

    private async Task<EndGameState?> HandleCallPatron(SimplePatronMove move)
    {
        if (!Enum.IsDefined(typeof(PatronId), move.PatronId))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Invalid patron selected.");
        }

        return _api.PatronActivation(move.PatronId) ?? await ConsumePotentialPendingMoves();
    }

    private async Task<EndGameState?> HandleChoice(SerializedChoice result)
    {
        switch (result.Type)
        {
            case Choice.DataType.CARD:
            {
                var (timeout, move) = await PlayWithTimeout();
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
                var (timeout, move) = await PlayWithTimeout();
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

    private (EndGameState, SerializedBoard) EndGame(EndGameState state)
    {
        state.AdditionalContext +=
            $"\nLast few moves for context:\n{string.Join('\n', _moveHistory.TakeLast(5).Select(m => m.ToString()))}";
        CurrentPlayer.GameEnd(state);
        EnemyPlayer.GameEnd(state);
        EndGameState = state;
        return (state, _api.GetSerializer());
    }
}
