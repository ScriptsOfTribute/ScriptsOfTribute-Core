using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.AI;

public class TalesOfTributeGame
{
    private ITalesOfTributeApi _api;
    private AI[] _players = new AI[2];
    public EndGameState? EndGameState { get; private set; }

    private AI CurrentPlayer => _players[(int)_api.CurrentPlayerId];
    private AI EnemyPlayer => _players[(int)_api.EnemyPlayerId];

    public TalesOfTributeGame(AI[] players, ITalesOfTributeApi api)
    {
        _api = api;
        _players[0] = players[0];
        _players[1] = players[1];
    }
    
    public EndGameState Play()
    {
        EndGameState? endGameState;
        while ((endGameState = _api.CheckWinner()) is null)
        {
            var startOfTurnResult = HandleStartOfTurnChoices();
            if (startOfTurnResult is not null)
            {
                return EndGame(startOfTurnResult);
            }

            Move move;
            do
            {
                move = CurrentPlayer.Play(_api.GetSerializer(), _api.GetListOfPossibleMoves());
                var result = HandleFreeMove(move);
                if (result is not null)
                {
                    return EndGame(result);
                }
            } while (move.Command != CommandEnum.END_TURN);

            _api.EndTurn();
        }

        return EndGame(endGameState);
    }


    private EndGameState? HandleStartOfTurnChoices()
    {
        var startOfTurnChoices = _api.HandleStartOfTurnChoices();

        if (startOfTurnChoices is null) return null;

        foreach (var choice in startOfTurnChoices.Consume())
        {
            if (choice is not Choice<Card> realChoice)
            {
                throw new Exception(
                    "There is something wrong in the engine! In case other start of turn choices were added (other than DESTROY), this needs updating.");
            }
                
            var result = HandleStartOfTurnChoice(realChoice);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private EndGameState? HandleStartOfTurnChoice(Choice<Card> choice)
    {
        var playersChoice = CurrentPlayer.Play(_api.GetSerializer(), _api.GetListOfPossibleMoves()) as MakeChoiceMove<Card>;

        if (playersChoice is null)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Start of turn choice for now is always DESTROY, so should be of type Card.");
        }

        var result = choice.Choose(playersChoice.Choices);

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

    private EndGameState? HandleFreeMove(Move move)
    {
        if (!_api.IsMoveLegal(move))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Illegal move.");
        }

        // This should probably be handled above (this move is not in legal moves), but you can never be to careful...
        if (move is BaseMakeChoiceMove)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "You don't have a pending choice.");
        }

        return move.Command switch
        {
            CommandEnum.PLAY_CARD => HandlePlayCard(move as SimpleCardMove),
            CommandEnum.ATTACK => HandleAttack(move as SimpleCardMove),
            CommandEnum.BUY_CARD => HandleBuyCard(move as SimpleCardMove),
            CommandEnum.END_TURN => null,
            CommandEnum.CALL_PATRON => HandleCallPatron(move as SimplePatronMove),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private EndGameState? HandlePlayCard(SimpleCardMove move)
    {
        var chain = _api.PlayCard(move.Card);

        return chain
            .Consume()
            .Select(HandleTopLevelResult)
            .FirstOrDefault(endGameState => endGameState is not null);
    }

    private EndGameState? HandleAttack(SimpleCardMove move)
    {
        if (_api.AttackAgent(move.Card) is Failure f)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, f.Reason);
        }

        return null;
    }

    private EndGameState? HandleBuyCard(SimpleCardMove move)
    {
        var chain = _api.BuyCard(move.Card);

        return chain
            .Consume()
            .Select(HandleTopLevelResult)
            .FirstOrDefault(endGameState => endGameState is not null);
    }

    private EndGameState? HandleCallPatron(SimplePatronMove move)
    {
        if (!Enum.IsDefined(typeof(PatronId), move.PatronId))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Invalid patron selected.");
        }

        if (_api.PatronActivation(move.PatronId) is Failure f)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, f.Reason);
        }

        return null;
    }

    private EndGameState? HandleTopLevelResult(PlayResult result)
    {
        return result switch
        {
            Success => null,
            Failure failure => new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, failure.Reason),
            _ => HandleChoice(result)
        };
    }

    private EndGameState? HandleChoice(PlayResult result)
    {
        do
        {
            switch (result)
            {
                case Choice<Card> choice:
                {
                    var move = CurrentPlayer.Play(_api.GetSerializer(), _api.GetListOfPossibleMoves());
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
                    var move = CurrentPlayer.Play(_api.GetSerializer(), _api.GetListOfPossibleMoves());
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
