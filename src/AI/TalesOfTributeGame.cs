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
                /*
                List<Card> t = _api.GetTavern();
                foreach (Card c in t){
                    Console.WriteLine((int)c.UniqueId);
                }
                */
                move = CurrentPlayer.Play(_api.GetSerializer(), _api.GetListOfPossibleMoves());
                var result = HandleMove(move);
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
        var playersChoice = CurrentPlayer.HandleStartOfTurnChoice(_api.GetSerializer(),
            SerializedChoice<Card>.FromChoice(choice));
            
        var result = choice.Choose(playersChoice);

        if (result is Failure f)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, f.Reason);
        }

        if (result is not Success)
        {
            throw new Exception(
                "There is something wrong in the engine! In case other start of turn choices were added (other than DESTROY), this needs updating.");
        }

        return null;
    }

    private EndGameState? HandleMove(Move move)
    {
        if (!_api.IsMoveLegal(move))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Illegal move.");
        }

        return move.Command switch
        {
            CommandEnum.PLAY_CARD => HandlePlayCard(move.Value),
            CommandEnum.ATTACK => HandleAttack(move.Value),
            CommandEnum.BUY_CARD => HandleBuyCard(move.Value),
            CommandEnum.END_TURN => null,
            CommandEnum.PATRON => HandleCallPatron(move.Value),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private EndGameState? HandlePlayCard(int uniqueId)
    {
        var chain = _api.PlayCard(uniqueId);

        return chain
            .Consume()
            .Select(HandleTopLevelResult)
            .FirstOrDefault(endGameState => endGameState is not null);
    }

    private EndGameState? HandleAttack(int uniqueId)
    {
        if (_api.AttackAgent(uniqueId) is Failure f)
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, f.Reason);
        }

        return null;
    }

    private EndGameState? HandleBuyCard(int uniqueId)
    {
        var chain = _api.BuyCard(uniqueId);

        return chain
            .Consume()
            .Select(HandleTopLevelResult)
            .FirstOrDefault(endGameState => endGameState is not null);
    }

    private EndGameState? HandleCallPatron(int id)
    {
        if (!Enum.IsDefined(typeof(PatronId), id))
        {
            return new EndGameState(_api.EnemyPlayerId, GameEndReason.INCORRECT_MOVE, "Invalid patron selected.");
        }

        if (_api.PatronActivation((PatronId)id) is Failure f)
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
                    var c = CurrentPlayer.HandleCardChoice(_api.GetSerializer(), SerializedChoice<Card>.FromChoice(choice));
                    result = choice.Choose(c);
                    break;
                }
                case Choice<EffectType> choice:
                {
                    var c= CurrentPlayer.HandleEffectChoice(_api.GetSerializer(), SerializedChoice<EffectType>.FromChoice(choice));
                    result = choice.Choose(c);
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
