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
    // Therefore, in this view, CurrentPlayer should always refer to the bot and EnemyPlayer always to the enemy,
    // even if it is his turn.
    public readonly FairSerializedPlayer CurrentPlayer;
    public readonly FairSerializedEnemyPlayer EnemyPlayer;
    
    // Completed actions are problematic, because they show what enemy player did on his turn, so may reveal
    // what cards he has.
    // TODO: Figure out what to do and probably hide some.
    public readonly List<CompletedAction> CompletedActions;

    // Bot should know what cards are left in tavern, but not the exact order.
    private List<UniqueCard>? _tavernAvailableCards;
    public List<UniqueCard> TavernAvailableCards =>
        _tavernAvailableCards ??= _board.TavernAvailableCards.OrderBy(c => c.CommonId).ToList();
    // What if there are choices for DESTROY for enemy player? We shouldn't reveal that, so return null
    // if turns changed.
    public SerializedChoice? PendingChoice => _endOfTurnHappened ? null : _board.PendingChoice;

    private readonly SerializedBoard _board;
    private readonly bool _endOfTurnHappened = false;

    public FairSerializedBoard(SerializedBoard board) : this(board, board.CurrentPlayer, board.EnemyPlayer)
    {
    }

    public FairSerializedBoard(SerializedBoard board, SerializedPlayer currentPlayer, SerializedPlayer enemyPlayer, bool endOfTurnHappened = false)
    {
        _board = board;
        CurrentPlayer = new FairSerializedPlayer(board.CurrentPlayer);
        EnemyPlayer = new FairSerializedEnemyPlayer(board.EnemyPlayer);
        _endOfTurnHappened = endOfTurnHappened;
    }

    public (FairSerializedBoard, List<Move>) ApplyState(Move move)
    {
        if (_endOfTurnHappened)
        {
            throw new Exception("You can't simulate any more moves as you've ended your turn.");
        }
        // This means bot wants to see what happens after end of this turn.
        // This means CurrentPlayer would be the bots' enemy, so he should have severely limited visibility.
        var (newBoard, newMoves) = _board.ApplyState(move);

        if (move.Command == CommandEnum.END_TURN)
        {
            return (new FairSerializedBoard(newBoard, _board.EnemyPlayer, _board.CurrentPlayer, true),
                new List<Move>());
        }

        return (new FairSerializedBoard(newBoard), newMoves);
    }
}
