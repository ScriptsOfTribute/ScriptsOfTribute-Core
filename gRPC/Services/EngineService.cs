using Grpc.Core;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Linq;

namespace ScriptsOfTributeGRPC;

public class EngineServiceAdapter : EngineService.EngineServiceBase
{
    private Dictionary<string, object> _states = new();
    private Dictionary<string, List<ScriptsOfTribute.Move>> _moves = new();
    public override Task<SimulationResult> ApplyMove(ApplyMoveRequest request, ServerCallContext context)
    {
        var gameState = GetState(request.StateId);
        var movesList = GetMovesList(request.StateId);
        (SeededGameState newGameState, List<ScriptsOfTribute.Move> moves) result = gameState switch
        {
            GameState gs => gs.ApplyMove(Mapper.MapMove(request.Move, gs.PendingChoice, movesList), request.Seed),
            SeededGameState sgs => sgs.ApplyMove(Mapper.MapMove(request.Move, sgs.PendingChoice, movesList), request.Seed),
            _ => throw new InvalidOperationException("Unknown game state type.")
        };
        RegisterState(result.newGameState.StateId, result.newGameState);
        RegisterMovesList(result.newGameState.StateId, result.moves);
        var response = new SimulationResult
        {
            GameState = Mapper.ToProto(result.newGameState),
        };
        response.PossibleMoves.AddRange(result.moves.Select(Mapper.MapMove).ToList());
        return Task.FromResult(response);
    }

    public override Task<Empty> ReleaseState(StateId request, ServerCallContext context)
    {
        //Console.WriteLine($"Releasing state {request.Id}");
        ReleaseState(request.Id);
        ReleaseMoves(request.Id);
        return Task.FromResult(new Empty());
    }

    public void RegisterState(string id, object state)
    {
        _states[id] = state;
    }

    public void RegisterMovesList(string id, List<ScriptsOfTribute.Move> moves)
    {
        _moves[id] = moves;
    }

    public object GetState(string id)
    {
        if (_states.TryGetValue(id, out var entry))
        {
            return entry;
        }
        throw new KeyNotFoundException($"State {id} not found");
    }

    public List<ScriptsOfTribute.Move> GetMovesList(string id)
    {
        if (_moves.TryGetValue(id, out var entry))
        {
            return entry;
        }
        throw new KeyNotFoundException($"State {id} not found");
    }

    public void ReleaseState(string id)
    {
        if (_states.TryGetValue(id, out var entry))
        {
            _states.Remove(id);
        }
    }
    public void ReleaseMoves(string id)
    {
        if (_moves.TryGetValue(id, out var entry))
        {
            _moves.Remove(id);
        }
    }

    public void CleanCache()
    {
        _states = new();
        _moves = new();
    }
}
