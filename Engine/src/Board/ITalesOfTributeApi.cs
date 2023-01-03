﻿using TalesOfTribute.Board.CardAction;
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

    public void MakeChoice(List<Card> choices);
    public void MakeChoice(Effect choice);

    void ActivateAgent(Card agent);
    void AttackAgent(Card agent);

    /// <summary>
    /// Activate Patron with patronId. Only CurrentPlayer can activate patron
    /// </summary>
    void PatronActivation(PatronId patronId);

    /// <summary>
    /// Buys card <c>card</c> in tavern for CurrentPlayer.
    /// Checks if CurrentPlayer has enough Coin and if no choice is pending.
    /// </summary>
    void BuyCard(Card card);

    /// <summary>
    /// Plays card <c>card</c> from hand for CurrentPlayer
    /// Checks if CurrentPlayer has this card in hand and if no choice is pending.
    /// </summary>
    void PlayCard(Card card);

    List<Move> GetListOfPossibleMoves();
    bool IsMoveLegal(Move playerMove);
    void EndTurn();

    /// <summary>
    /// Returns ID or player who won the game. If game is still going it returns <c>PlayerEnum.NO_PLAYER_SELECTED</c>
    /// </summary>
    EndGameState? CheckWinner();
}
