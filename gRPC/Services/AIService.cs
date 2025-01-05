using Grpc.Core;
using Grpc.Net.Client;
using System;
using ScriptsOfTribute;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;
using System.Runtime.InteropServices;


namespace ScriptsOfTributeGRPC;

public class AIServiceAdapter : IDisposable
{
    private readonly AIService.AIServiceClient _client;
    private readonly GrpcChannel _channel;
    private string _host;
    private int _port;
    public AIServiceAdapter(string host="localhost", int port=50000)
    {
        _host = host;
        _port = port;
        string address = $"http://{host}:{port}";
        _channel = GrpcChannel.ForAddress(address);
        _client = new AIService.AIServiceClient(_channel);
    }

    public string RegisterBot()
    {
        var response = _client.RegisterBot(new Empty() { });
        return response.Name;
    }

    public void PregamePrepare()
    {
        _client.PregamePrepare(new Empty() { });
    }

    public ScriptsOfTribute.PatronId SelectPatron(List<ScriptsOfTribute.PatronId> availablePatrons, int round)
    {
        var request = new SelectPatronRequest
        {
            Round = round
        };

        request.AvailablePatrons.AddRange(availablePatrons.Select(patronId => (PatronId)((int)patronId)).ToList());

        var response = _client.SelectPatron(request);
        return (ScriptsOfTribute.PatronId)((int)response.PatronId);
    }

    public ScriptsOfTribute.Move Play(ScriptsOfTribute.Serializers.GameState gameState, List<ScriptsOfTribute.Move> availableMoves, TimeSpan remainingTime)
    {
        var jsonString = gameState.SerializeGameState().ToString();
        var moves = availableMoves.Select(MapMove).ToList();
        var time = (long)remainingTime.TotalMilliseconds;

        var request = new PlayRequest
        {
            GameState = new GameState { GameStateJson = jsonString },
            RemainingTimeMs = time
        };
        request.PossibleMoves.AddRange(moves);

        var response = _client.Play(request);

        return MapMove(response, gameState.PendingChoice);
    }

    public void GameEnd(ScriptsOfTribute.Board.EndGameState state, FullGameState finalBoardState)
    {
        var gameState = new ScriptsOfTribute.Serializers.GameState(finalBoardState);

        var request = new GameEndRequest
        {
            State = new EndGameState { Winner = state.Winner.ToString(), Reason = state.Reason.ToString(), AdditionalContext = state.AdditionalContext},
            FinalBoardState = new GameState { GameStateJson = gameState.SerializeGameState().ToString() },
        };
    }

    public void CloseConnection()
    {
        _client.CloseServer(new Empty() { });
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }

    private Move MapMove(ScriptsOfTribute.Move move)
    {
        if (move == null)
        {
            return new Move();
        }

        if (move.Command == CommandEnum.END_TURN)
        {
            var newMove = new Move();
            newMove.Basic = new BasicMove 
            {
                Command = MoveEnum.EndTurn
            };
            return newMove;
        }

        if (move is ScriptsOfTribute.SimpleCardMove cardMove)
        {
            var newMove = new Move();
            newMove.CardMove = new SimpleCardMove
            {
                Command = (MoveEnum)((int)cardMove.Command),
                CardUniqueId = cardMove.Card.UniqueId.Value
            };
            return newMove;
        }

        if (move is ScriptsOfTribute.SimplePatronMove patronMove)
        {
            var newMove = new Move();
            newMove.PatronMove = new SimplePatronMove
            {
                Command = MoveEnum.CallPatron,
                PatronId = (PatronId)((int)patronMove.PatronId)
            };
            return newMove;
        }

        if (move is ScriptsOfTribute.MakeChoiceMoveUniqueCard choiceCardMove || move is ScriptsOfTribute.MakeChoiceMove<UniqueCard> genericChoiceCardMove)
        {
            var newMove = new Move();
            newMove.CardChoiceMove = new MakeChoiceMoveUniqueCard
            {
                Command = MoveEnum.MakeChoice,
            };

            var choices = move is ScriptsOfTribute.MakeChoiceMoveUniqueCard m
                ? m.Choices
                : ((ScriptsOfTribute.MakeChoiceMove<UniqueCard>)move).Choices;

            newMove.CardChoiceMove.CardsUniqueIds.AddRange(choices.Select(card => card.UniqueId.Value).ToArray());
            return newMove;
        }

        if (move is ScriptsOfTribute.MakeChoiceMoveUniqueEffect choiceEffectMove || move is ScriptsOfTribute.MakeChoiceMove<UniqueEffect> genericChoiceEffectMove)
        {
            var newMove = new Move();
            newMove.EffectChoiceMove = new MakeChoiceMoveUniqueEffect
            {
                Command = MoveEnum.MakeChoice,
            };

            var choices = move is ScriptsOfTribute.MakeChoiceMoveUniqueEffect m
                ? m.Choices
                : ((ScriptsOfTribute.MakeChoiceMove<UniqueEffect>)move).Choices;

            newMove.EffectChoiceMove.Effects.AddRange(choices.Select(eff => eff.ToSimpleString()).ToArray());
            return newMove;
        }

        throw new ArgumentException("Undefined behaviour in AIService.MapMove method");
    }

    private ScriptsOfTribute.Move MapMove(Move move, SerializedChoice? PendingChoice)
    {
        if (move == null)
        {
            return ScriptsOfTribute.Move.EndTurn();
        }

        if (move.Basic != null && move.Basic.Command == MoveEnum.EndTurn)
        {
            return ScriptsOfTribute.Move.EndTurn();
        }

        if (move.CardMove != null)
        {
            var cardReferencedTo = GlobalCardDatabase.Instance.AllCards.First(card => move.CardMove.CardUniqueId == card.UniqueId.Value);
            return move.CardMove.Command switch
            {
                MoveEnum.PlayCard => ScriptsOfTribute.Move.PlayCard(cardReferencedTo),
                MoveEnum.ActivateAgent => ScriptsOfTribute.Move.ActivateAgent(cardReferencedTo),
                MoveEnum.BuyCard => ScriptsOfTribute.Move.BuyCard(cardReferencedTo),
                MoveEnum.Attack => ScriptsOfTribute.Move.Attack(cardReferencedTo),
                _ => throw new ArgumentException("Undefined behaviour in AIService.MapMove method")
            };
        }

        if (move.PatronMove != null)
        {
            var patronReferenced = (ScriptsOfTribute.PatronId)((int)move.PatronMove.PatronId);
            return ScriptsOfTribute.Move.CallPatron(patronReferenced);
        }

        if (move.CardChoiceMove != null)
        {
            var cardsReferencedTo = GlobalCardDatabase.Instance.AllCards.Where(card => move.CardChoiceMove.CardsUniqueIds.Contains(card.UniqueId.Value)).ToList();
            return ScriptsOfTribute.Move.MakeChoice(cardsReferencedTo); 
        }

        if (move.EffectChoiceMove != null && PendingChoice != null)
        {
            var referencedEffects = move.EffectChoiceMove.Effects;
            var pickedEffects = PendingChoice.PossibleEffects.Where(eff => referencedEffects.Contains(eff.ToSimpleString())).ToList();
            return ScriptsOfTribute.Move.MakeChoice(pickedEffects);
        }

        throw new ArgumentException("Undefined behaviour in AIService.MapMove method");
    }
}
