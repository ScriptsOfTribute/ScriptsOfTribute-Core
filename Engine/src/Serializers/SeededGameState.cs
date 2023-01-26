using TalesOfTribute.Board;
using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Board.Cards;

namespace TalesOfTribute.Serializers;

public class SeededGameState
{
    public SerializedPlayer CurrentPlayer => _board.CurrentPlayer;
    public SerializedPlayer EnemyPlayer => _board.EnemyPlayer;
    public PatronStates PatronStates => _board.PatronStates;
    public List<PatronId> Patrons => PatronStates.All.Select(p => p.Key).ToList();
    public List<UniqueCard> TavernAvailableCards => _board.TavernAvailableCards;
    public List<UniqueCard> TavernCards => _board.TavernCards;
    public BoardState BoardState => _board.BoardState;
    public SerializedChoice? PendingChoice => _board.PendingChoice;
    public ComboStates ComboStates => _board.ComboStates;
    public List<UniqueBaseEffect> UpcomingEffects => _board.UpcomingEffects;
    public List<UniqueBaseEffect> StartOfNextTurnEffects => _board.StartOfNextTurnEffects;
    public List<CompletedAction> CompletedActions => _board.CompletedActions;
    public EndGameState? GameEndState => _board.GameEndState;
    public ulong InitialSeed => _board.InitialSeed;
    public ulong CurrentSeed => _board.CurrentSeed;

    private FullGameState _board;

    public SeededGameState(FullGameState board, ulong seed)
    {
        var rng = new SeededRandom(seed);

        var newCurrentPlayer = ScramblePlayerCards(rng, board.CurrentPlayer);
        var newEnemyPlayer = ScramblePlayerCards(rng, board.EnemyPlayer);

        // NOTE: What if player creates this class after simulating some moves? Then he can have unknown cards here,
        // is that a problem? In theory it should work.
        var newTavernCards = board.TavernCards.OrderBy(_ => rng.Next()).ToList();

        _board = new FullGameState(newCurrentPlayer, newEnemyPlayer, board.PatronStates, board.TavernAvailableCards,
            newTavernCards, board.BoardState, board.PendingChoice, board.ComboStates, board.UpcomingEffects,
            board.StartOfNextTurnEffects, board.CompletedActions, board.GameEndState, seed, seed, true);
    }

    public SeededGameState(FullGameState board)
    {
        _board = board;
    }

    private SerializedPlayer ScramblePlayerCards(SeededRandom rng, SerializedPlayer player)
    {
        var newCooldown = player.CooldownPile.OrderBy(_ => rng.Next()).ToList();
        var knownUpcomingCardAmount = player.KnownUpcomingDraws.Count;
        var upcomingDraw = player.DrawPile.Take(knownUpcomingCardAmount);
        var restOfDraw = player.DrawPile.Skip(knownUpcomingCardAmount);
        var handSize = player.Hand.Count;
        var handAndDraw = player.Hand.Concat(restOfDraw).OrderBy(_ => rng.Next()).ToList();
        var newHand = handAndDraw.Take(handSize).ToList();
        var newDraw = upcomingDraw.Concat(handAndDraw.Skip(handSize)).ToList();
        return new SerializedPlayer(
            player.PlayerID, newHand, newDraw, newCooldown, player.Played, player.Agents, player.Power,
            player.PatronCalls,
            player.Coins, player.Prestige);
    }

    public (SeededGameState, List<Move>) ApplyState(Move move)
    {
        var (newBoard, newMoves) = _board.ApplyState(move);
        return (new SeededGameState(newBoard), newMoves);
    }
}
