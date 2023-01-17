using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Board.Cards;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.Board;

public interface ITalesOfTributeApi
{
    int TurnCount { get; }
    PlayerEnum CurrentPlayerId { get; }
    PlayerEnum EnemyPlayerId { get; }
    public BoardState BoardState { get; }
    public SerializedChoice? PendingChoice { get; }
    SerializedBoard GetSerializer();
    public TextWriter LogTarget { get; set; }

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

    public void Log(string message);
    public void Log(PlayerEnum player, string message);
    public void Log(List<(DateTime, string)> messages);
    public void Log(PlayerEnum player, List<(DateTime, string)> messages);
}
