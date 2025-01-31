using Grpc.Core;
using ScriptsOfTribute;
using ScriptsOfTribute.Serializers;
using System;
using System.Collections.Concurrent;


namespace ScriptsOfTributeGRPC;

public class EngineServiceAdapter : EngineService.EngineServiceBase
{
    private readonly Dictionary<string, GameState> _states = new();
    public override Task<SimulationResult> ApplyMove(ApplyMoveRequest request, ServerCallContext context)
    {
        var gameState = GetState(request.StateId);
        var (newGameState, moves) = gameState.ApplyMove(Mapper.MapMove(request.Move, gameState.PendingChoice), request.Seed);
        var gameStateProto = Mapper.ToProto(newGameState);
        var response = new SimulationResult
        {
            GameState = gameStateProto,
        };
        response.PossibleMoves.AddRange(moves.Select(Mapper.MapMove).ToList());
        return Task.FromResult(response);
    }

    public override Task<GameStateProto> GetState(StateId request, ServerCallContext context)
    {
        var gameState = GetState(request.Id);
        var response = Mapper.ToProto(gameState);
        return Task.FromResult(response);
    }

    public override Task<Empty> ReleaseState(StateId request, ServerCallContext context)
    {
        ReleaseState(request.Id);
        return Task.FromResult(new Empty());
    }

    public void RegisterState(GameState state)
    {
        _states[state.StateId] = state;
    }

    public GameState GetState(string id)
    {
        if (_states.TryGetValue(id, out var entry))
        {
            _states[id] = entry;
            return entry;
        }
        throw new KeyNotFoundException("State not found");
    }

    public void ReleaseState(string id)
    {
        if (_states.TryGetValue(id, out var entry))
        {
            _states.Remove(id);
        }
    }
}
