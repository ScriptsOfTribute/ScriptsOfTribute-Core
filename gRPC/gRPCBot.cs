using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ScriptsOfTribute;
using ScriptsOfTribute.AI;

namespace ScriptsOfTributeGRPC;

public class gRPCBot : AI
{
    private AIServiceAdapter _AIService;
    private EngineServiceAdapter _engineService;
    private GrpcServer _grpcServer;
    public string Name { get; private set; }

    public gRPCBot(string host="localhost", int clientPort=50000, int serverPort = 49000)
    {
        //Console.Error.WriteLine($"Listening on clientPort: {clientPort}");
        //Console.Error.WriteLine($"Listening on serverPort: {serverPort}");
        _AIService = new AIServiceAdapter(host, clientPort);
        _engineService = new EngineServiceAdapter();
        _grpcServer = new GrpcServer(host, serverPort, _engineService);

        Name = RegisterBot();
        //Console.WriteLine($"Created bot {Name}");
    }

    public string RegisterBot()
    {
        return _AIService.RegisterBot();
    }

    public override void PregamePrepare()
    {
        _AIService.PregamePrepare();
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        var patronId = _AIService.SelectPatron(availablePatrons, round);
        return patronId;
    }

    public override ScriptsOfTribute.Move Play(
        ScriptsOfTribute.Serializers.GameState gameState,
        List<ScriptsOfTribute.Move> possibleMoves,
        TimeSpan remainingTime
    )
    {
        _engineService.RegisterState(gameState.StateId, gameState);
        _engineService.RegisterMovesList(gameState.StateId, possibleMoves);
        var move = _AIService.Play(gameState, possibleMoves, remainingTime);
        return move;
    }

    public override void GameEnd(ScriptsOfTribute.Board.EndGameState state, FullGameState? finalBoardState)
    {
        _engineService.CleanCache();
        if ( finalBoardState != null ) 
        {
            _AIService.GameEnd(state, finalBoardState);
        }
    }

    public void CloseConnection()
    {
        //Console.Error.WriteLine($"Closing {Name}");
        _AIService.CloseConnection();
        _grpcServer?.Dispose();
    }
}
