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
        //Console.WriteLine($"Requested on http://{_host}:{_port}");
        return response.Name;
    }

    public void PregamePrepare()
    {
        _client.PregamePrepare(new Empty() { });
    }

    public PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        var request = new SelectPatronRequest
        {
            Round = round
        };

        request.AvailablePatrons.AddRange(availablePatrons.Select(patronId => (PatronIdProto)(patronId)).ToList());

        var response = _client.SelectPatron(request);
        return (PatronId)(response.PatronId);
    }

    public ScriptsOfTribute.Move Play(GameState gameState, List<ScriptsOfTribute.Move> availableMoves, TimeSpan remainingTime)
    {
        var grpcGameState = Mapper.ToProto(gameState);
        var moves = availableMoves.Select(Mapper.MapMove).ToList();
        var time = (long)remainingTime.TotalMilliseconds;

        var request = new PlayRequest
        {
            GameState = grpcGameState,
            RemainingTimeMs = time
        };
        request.PossibleMoves.AddRange(moves);
        var response = _client.Play(request);
        var move = Mapper.MapMove(response, gameState.PendingChoice, availableMoves);
        return move;
    }

    public void GameEnd(ScriptsOfTribute.Board.EndGameState state, FullGameState finalBoardState)
    {
        var gameState = Mapper.ToProto(new GameState(finalBoardState));

        var request = new GameEndRequest
        {
            State = new EndGameState { Winner = state.Winner.ToString(), Reason = state.Reason.ToString(), AdditionalContext = state.AdditionalContext},
            FinalBoardState = gameState,
        };
        _client.GameEnd(request);
    }

    public void CloseConnection()
    {
        _client.CloseServer(new Empty() { });
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }

    
}
