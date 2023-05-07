﻿using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute.Serializers;

public class GameState
{
    public PatronStates PatronStates => _board.PatronStates;
    public List<PatronId> Patrons => PatronStates.All.Select(p => p.Key).ToList();
    public List<UniqueCard> TavernAvailableCards => _board.TavernAvailableCards;
    public BoardState BoardState => _board.BoardState;
    public ComboStates ComboStates => _board.ComboStates;
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
    public List<CompletedAction> CompletedActions => _board.CompletedActions;

    // Bot should know what cards are left in tavern, but not the exact order.
    private List<UniqueCard>? _tavernCards;
    public List<UniqueCard> TavernCards =>
        _tavernCards ??= _board.TavernCards.OrderBy(c => c.CommonId).ToList();
    // What if there are choices for DESTROY for enemy player? We shouldn't reveal that, so return null
    // if turns changed.
    public SerializedChoice? PendingChoice => _board.PendingChoice;

    private readonly FullGameState _board;

    public GameState(FullGameState board) : this(board, board.CurrentPlayer, board.EnemyPlayer)
    {
    }

    public GameState(FullGameState board, SerializedPlayer currentPlayer, SerializedPlayer enemyPlayer)
    {
        _board = board;
        CurrentPlayer = new FairSerializedPlayer(currentPlayer);
        EnemyPlayer = new FairSerializedEnemyPlayer(enemyPlayer);
    }

    public SeededGameState ToSeededGameState(ulong seed)
    {
        return new SeededGameState(_board, seed);
    }

    public (SeededGameState, List<Move>) ApplyMove(Move move, ulong seed)
    {
        var (newBoard, newMoves) = _board.ApplyMove(move);

        return (new SeededGameState(newBoard, seed), newMoves);
    }
}
