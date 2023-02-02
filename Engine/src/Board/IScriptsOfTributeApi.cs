using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.utils;

namespace ScriptsOfTribute.Board;

public interface IScriptsOfTributeApi
{
    int TurnCount { get; }
    public int TurnMoveCount { get; }
    PlayerEnum CurrentPlayerId { get; }
    PlayerEnum EnemyPlayerId { get; }
    public BoardState BoardState { get; }
    public SerializedChoice? PendingChoice { get; }
    FullGameState GetFullGameState();
    public Logger Logger { get; }

    public EndGameState? MakeChoice(List<UniqueCard> choices);
    public EndGameState? MakeChoice(UniqueEffect choice);

    EndGameState? ActivateAgent(UniqueCard agent);
    EndGameState? AttackAgent(UniqueCard agent);

    /// <summary>
    /// Activate Patron with patronId. Only CurrentPlayer can activate patron
    /// </summary>
    EndGameState? PatronActivation(PatronId patronId);

    /// <summary>
    /// Buys card <c>card</c> in tavern for CurrentPlayer.
    /// Checks if CurrentPlayer has enough Coin and if no choice is pending.
    /// </summary>
    EndGameState? BuyCard(UniqueCard card);

    /// <summary>
    /// Plays card <c>card</c> from hand for CurrentPlayer
    /// Checks if CurrentPlayer has this card in hand and if no choice is pending.
    /// </summary>
    EndGameState? PlayCard(UniqueCard card);

    List<Move> GetListOfPossibleMoves();
    bool IsMoveLegal(Move playerMove);
    EndGameState? EndTurn();

    /// <summary>
    /// Returns ID or player who won the game. If game is still going it returns <c>PlayerEnum.NO_PLAYER_SELECTED</c>
    /// </summary>
    EndGameState? CheckWinner();
}
