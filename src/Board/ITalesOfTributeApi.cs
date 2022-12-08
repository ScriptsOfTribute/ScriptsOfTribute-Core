namespace TalesOfTribute.Board;

public interface ITalesOfTributeApi
{
    PlayerEnum CurrentPlayerId { get; }
    PlayerEnum EnemyPlayerId { get; }
    SerializedBoard GetSerializer();
    ExecutionChain? HandleStartOfTurnChoices();
    BoardState GetState();

    /// <summary>
    /// Get cards in hand of current player
    /// </summary>
    List<Card> GetHand();

    /// <summary>
    /// Get played cards of current player
    /// </summary>
    List<Card> GetPlayedCards();

    /// <summary>
    /// Get played cards of <c>playerId player</c>
    /// </summary>
    /// <param name="playerId">ID of player</param>
    List<Card> GetPlayedCards(PlayerEnum playerId);

    /// <summary>
    /// Get draw pile of current player
    /// </summary>
    List<Card> GetDrawPile();

    /// <summary>
    /// Get drawpile of <c>playerId player</c>
    /// </summary>
    /// <param name="playerId">ID of player</param>
    List<Card> GetDrawPile(PlayerEnum playerId);

    /// <summary>
    /// Get cooldown pile of current player
    /// </summary>
    List<Card> GetCooldownPile();

    /// <summary>
    /// Get cooldown pile of <c>playerId player</c>
    /// </summary>
    /// <param name="playerId">ID of player</param>
    List<Card> GetCooldownPile(PlayerEnum playerId);

    /// <summary>
    /// Get currently available cards from tavern
    /// </summary>
    List<Card> GetTavern();

    /// <summary>
    /// Get cards from tavern that player with playerId can buy
    /// </summary>
    List<Card> GetAffordableCardsInTavern(PlayerEnum playerId);

    /// <summary>
    /// Get list of agents currently on board for player with playerId
    /// </summary>
    List<Agent> GetAgents(PlayerEnum playerId);

    /// <summary>
    /// Get list of agents currently on board for current player
    /// </summary>
    List<Agent> GetAgents();

    /// <summary>
    /// Get list of agents currently on board for player with playerId
    /// that are activated
    /// </summary>
    List<Agent> GetActiveAgents(PlayerEnum playerId);

    List<Agent> GetActiveAgents();
    ExecutionChain ActivateAgent(Card agent);
    ISimpleResult AttackAgent(Card agent);
    ISimpleResult AttackAgent(int uniqueId);

    /// <summary>
    /// Activate Patron with patronId. Only CurrentPlayer can activate patron
    /// </summary>
    PlayResult PatronActivation(PatronId patronId);

    /// <summary>
    /// Return <type>PlayerEnum</type> which states which player is favored
    /// by Patron with patronId
    /// </summary>
    PlayerEnum GetLevelOfFavoritism(PatronId patronId);

    /// <summary>
    /// Buys card <c>card</c> in tavern for CurrentPlayer.
    /// Checks if CurrentPlayer has enough Coin and if no choice is pending.
    /// </summary>
    ExecutionChain BuyCard(Card card);

    ExecutionChain BuyCard(int uniqueId);

    /// <summary>
    /// Plays card <c>card</c> from hand for CurrentPlayer
    /// Checks if CurrentPlayer has this card in hand and if no choice is pending.
    /// </summary>
    ExecutionChain PlayCard(Card card);

    ExecutionChain PlayCard(int uniqueId);
    List<Move> GetListOfPossibleMoves();
    bool IsMoveLegal(Move playerMove);
    void EndTurn();

    /// <summary>
    /// Returns ID or player who won the game. If game is still going it returns <c>PlayerEnum.NO_PLAYER_SELECTED</c>
    /// </summary>
    EndGameState? CheckWinner();
}