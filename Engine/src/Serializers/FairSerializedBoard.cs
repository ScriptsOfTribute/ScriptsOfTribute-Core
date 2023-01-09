using TalesOfTribute.Board;
using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Board.Cards;

namespace TalesOfTribute.Serializers;

public class FairSerializedBoard
{
    public PatronStates PatronStates => _board.PatronStates;
    public List<PatronId> Patrons => PatronStates.All.Select(p => p.Key).ToList();
    public List<UniqueCard> TavernCards => _board.TavernCards;
    public BoardState BoardState => _board.BoardState;
    public ComboStates ComboStates => _board.ComboStates;
    public ulong Seed => _board.Seed;
    public List<UniqueBaseEffect> UpcomingEffects => _board.UpcomingEffects;
    public List<UniqueBaseEffect> StartOfNextTurnEffects => _board.StartOfNextTurnEffects;
    public EndGameState? GameEndState => _board.GameEndState;

    // Bot shouldn't have access to hand etc of enemy player.
    // Furthermore, when he simulates end turn (Move.EndTurn) he shouldn't have access to any information
    // about the other player and shouldn't be allowed to do basically anything.
    public readonly FairSerializedPlayer CurrentPlayer;
    public readonly FairSerializedEnemyPlayer EnemyPlayer;
    
    // Completed actions are problematic, because they show what enemy player did on his turn, so may reveal
    // what cards he has.
    public readonly List<CompletedAction> CompletedActions;

    // Bot should know what cards are left in tavern, but not the exact order.
    private List<UniqueCard>? _tavernAvailableCards;
    public List<UniqueCard> TavernAvailableCards =>
        _tavernAvailableCards ??= _board.TavernAvailableCards.OrderBy(c => c.CommonId).ToList();
    // Well, what if there are choices for DESTROY for enemy player? We shouldn't reveal that.
    public SerializedChoice? PendingChoice => _board.PendingChoice;

    private readonly SerializedBoard _board;

    public FairSerializedBoard(SerializedBoard board)
    {
        _board = board;
        CurrentPlayer = new FairSerializedPlayer(board.CurrentPlayer);
        EnemyPlayer = new FairSerializedEnemyPlayer(board.EnemyPlayer);
    }
}
