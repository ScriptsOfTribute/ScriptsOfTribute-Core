using System;
using System.Collections.Generic;
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

    public gRPCBot(string host="localhost", int clientPort=50000, int serverPort = 49000)
    {
        _AIService = new AIServiceAdapter(host, clientPort);
        _engineService = new EngineServiceAdapter();
        _grpcServer = new GrpcServer(host, serverPort, _engineService);
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
        _engineService.RegisterState(gameState);
        var move = _AIService.Play(gameState, possibleMoves, remainingTime);
        return move;
    }

    public override void GameEnd(ScriptsOfTribute.Board.EndGameState state, FullGameState? finalBoardState)
    {
        if ( finalBoardState != null ) 
        {
            _AIService.GameEnd(state, finalBoardState);
        }
    }

    public void CloseConnection()
    {
        _AIService.CloseConnection();
        _grpcServer?.Dispose();
    }
}
