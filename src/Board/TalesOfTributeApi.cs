using TalesOfTribute.Serializers;

namespace TalesOfTribute.Board;

public class TalesOfTributeApi : ITalesOfTributeApi
{
    public PlayerEnum CurrentPlayerId => _boardManager.CurrentPlayer.ID;
    public PlayerEnum EnemyPlayerId => _boardManager.EnemyPlayer.ID;
    public BoardState BoardState => _boardManager.State;
    
    private readonly BoardManager _boardManager;

    // Constructors
    public TalesOfTributeApi(BoardManager boardManager)
    {
        // what is the use case of this??
        _boardManager = boardManager;
    }

    /// <summary>
    /// Initialize board with selected patrons. patrons argument should contain PatronId.TREASURY
    /// but it handles situation when user doesn't put it.
    /// </summary>
    public TalesOfTributeApi(PatronId[] patrons)
    {
        if (!Array.Exists(patrons, p => p == PatronId.TREASURY))
        {
            // In case user forgets about Treasury (she/he shouldnt)
            List<PatronId> tempList = patrons.ToList();
            tempList.Add(PatronId.TREASURY);
            patrons = tempList.ToArray();
        }
        _boardManager = new BoardManager(patrons);
        _boardManager.SetUpGame();
    }

    // Serialization
    public SerializedBoard GetSerializer()
    {
        return _boardManager.SerializeBoard();
    }

    public SerializedPlayer GetPlayer(PlayerEnum playerId)
    {
        if (playerId == CurrentPlayerId)
        {
            return new SerializedPlayer(_boardManager.CurrentPlayer);
        }
        else
        {
            return new SerializedPlayer(_boardManager.EnemyPlayer);
        }
    }

    public ExecutionChain? HandleStartOfTurnChoices()
    {
        return _boardManager.HandleStartOfTurnChoices();
    }

    public BoardState GetState()
    {
        return _boardManager.State;
    }

    // Get cards

    /// <summary>
    /// Get cards in hand of current player
    /// </summary>
    public List<Card> GetHand()
    {
        return _boardManager.CurrentPlayer.Hand;
    }

    /// <summary>
    /// Get played cards of current player
    /// </summary>
    public List<Card> GetPlayedCards()
    {
        return _boardManager.CurrentPlayer.Played;
    }

    /// <summary>
    /// Get draw pile of current player
    /// </summary>
    public List<Card> GetDrawPile()
    {
        return _boardManager.CurrentPlayer.DrawPile;
    }

    /// <summary>
    /// Get cooldown pile of current player
    /// </summary>
    public List<Card> GetCooldownPile()
    {
        return _boardManager.CurrentPlayer.CooldownPile;
    }

    /// <summary>
    /// Get played cards of <c>playerId player</c>
    /// </summary>
    /// <param name="playerId">ID of player</param>
    public List<Card> GetPlayedCards(PlayerEnum playerId)
    {
        if (playerId == _boardManager.CurrentPlayer.ID)
        {
            return _boardManager.CurrentPlayer.Played;
        }
        else
        {
            return _boardManager.EnemyPlayer.Played;
        }
    }

    /// <summary>
    /// Get cooldown pile of <c>playerId player</c>
    /// </summary>
    /// <param name="playerId">ID of player</param>
    public List<Card> GetCooldownPile(PlayerEnum playerId)
    {
        if (playerId == _boardManager.CurrentPlayer.ID)
        {
            return _boardManager.CurrentPlayer.CooldownPile;
        }
        else
        {
            return _boardManager.EnemyPlayer.CooldownPile;
        }
    }

    /// <summary>
    /// Get drawpile of <c>playerId player</c>
    /// </summary>
    /// <param name="playerId">ID of player</param>
    public List<Card> GetDrawPile(PlayerEnum playerId)
    {
        if (playerId == _boardManager.CurrentPlayer.ID)
        {
            return _boardManager.CurrentPlayer.DrawPile;
        }
        else
        {
            return _boardManager.EnemyPlayer.DrawPile;
        }
    }

    // Tavern

    /// <summary>
    /// Get currently available cards from tavern
    /// </summary>
    public List<Card> GetTavern()
    {
        return _boardManager.GetAvailableTavernCards();
    }

    /// <summary>
    /// Get cards from tavern that player with playerId can buy
    /// </summary>
    public List<Card> GetAffordableCardsInTavern(PlayerEnum playerId)
    {
        if (playerId == _boardManager.CurrentPlayer.ID)
        {
            return _boardManager.GetAffordableCards(_boardManager.CurrentPlayer.CoinsAmount);
        }
        else
        {
            return _boardManager.GetAffordableCards(_boardManager.EnemyPlayer.CoinsAmount);
        }
    }

    // Agents related

    /// <summary>
    /// Get list of agents currently on board for player with playerId
    /// </summary>
    public List<Agent> GetAgents(PlayerEnum playerId)
    {
        if (playerId == _boardManager.CurrentPlayer.ID)
        {
            return _boardManager.CurrentPlayer.Agents;
        }
        else
        {
            return _boardManager.EnemyPlayer.Agents;
        }
    }

    /// <summary>
    /// Get list of agents currently on board for current player
    /// </summary>
    public List<Agent> GetAgents()
    {
        return _boardManager.CurrentPlayer.Agents;
    }

    /// <summary>
    /// Get list of agents currently on board for player with playerId
    /// that are activated
    /// </summary>
    public List<Agent> GetActiveAgents(PlayerEnum playerId)
    {
        if (playerId == _boardManager.CurrentPlayer.ID)
        {
            return _boardManager.CurrentPlayer.Agents.FindAll(agent => !agent.Activated);
        }
        else
        {
            return _boardManager.EnemyPlayer.Agents.FindAll(agent => !agent.Activated);
        }
    }

    public List<Agent> GetActiveAgents()
    {
        return _boardManager.CurrentPlayer.Agents.FindAll(agent => !agent.Activated);
    }

    public ExecutionChain ActivateAgent(Card agent)
        => _boardManager.ActivateAgent(agent);

    public ISimpleResult AttackAgent(Card agent)
        => _boardManager.AttackAgent(agent);

    public ISimpleResult AttackAgent(int uniqueId)
    {
        return _boardManager.AttackAgent(_boardManager.EnemyPlayer.GetCardByUniqueId(uniqueId));
    }

    // Patron related

    /// <summary>
    /// Activate Patron with patronId. Only CurrentPlayer can activate patron
    /// </summary>
    public PlayResult PatronActivation(PatronId patronId)
    {
        return _boardManager.PatronCall(patronId);
    }

    /// <summary>
    /// Return <type>PlayerEnum</type> which states which player is favored
    /// by Patron with patronId
    /// </summary>
    public PlayerEnum GetLevelOfFavoritism(PatronId patronId)
    {
        return _boardManager.GetPatronFavorism(patronId);
    }

    // cards related

    /// <summary>
    /// Buys card <c>card</c> in tavern for CurrentPlayer.
    /// Checks if CurrentPlayer has enough Coin and if no choice is pending.
    /// </summary>
    public ExecutionChain BuyCard(Card card)
    {
        return _boardManager.BuyCard(card);
    }
        
    public ExecutionChain BuyCard(int uniqueId)
    {
        return _boardManager.BuyCard(_boardManager.CurrentPlayer.GetCardByUniqueId(uniqueId));
    }

    /// <summary>
    /// Plays card <c>card</c> from hand for CurrentPlayer
    /// Checks if CurrentPlayer has this card in hand and if no choice is pending.
    /// </summary>
    public ExecutionChain PlayCard(Card card)
    {
        return _boardManager.PlayCard(card);
    }
        
    public ExecutionChain PlayCard(int uniqueId)
    {
        return PlayCard(_boardManager.CurrentPlayer.GetCardByUniqueId(uniqueId));
    }

    //others

    public List<Move> GetListOfPossibleMoves()
    {
        switch (_boardManager.CurrentPlayer.PendingExecutionChain?.PendingChoice)
        {
            case Choice<Card> cardChoice:
            {
                var result = new List<Move>();
                for (var i = cardChoice.MinChoiceAmount; i <= cardChoice.MaxChoiceAmount; i++)
                {
                    result.AddRange(cardChoice.PossibleChoices.GetCombinations(i).Select(Move.MakeChoice));
                }

                return result;
            }
            case Choice<EffectType> effectChoice:
            {
                var result = new List<Move>();
                for (var i = effectChoice.MinChoiceAmount; i <= effectChoice.MaxChoiceAmount; i++)
                {
                    result.AddRange(effectChoice.PossibleChoices.GetCombinations(i).Select(Move.MakeChoice));
                }

                return result;
            }
        }

        List<Move> possibleMoves = new List<Move>();
        Player currentPlayer = _boardManager.CurrentPlayer;
        Player enemyPlayer = _boardManager.EnemyPlayer;

        foreach (Card card in currentPlayer.Hand)
        {
            possibleMoves.Add(Move.PlayCard(card));
        }

        // TODO: Shouldn't there be 'activate agent' command?
        foreach (Agent agent in currentPlayer.Agents)
        {
            if (!agent.Activated)
            {
                possibleMoves.Add(Move.PlayCard(agent.RepresentingCard));
            }
        }

        var tauntAgents = enemyPlayer.Agents.FindAll(agent => agent.RepresentingCard.Taunt);
        if (currentPlayer.PowerAmount > 0)
        {
            if (tauntAgents.Any())
            {
                foreach (Agent agent in tauntAgents)
                {
                    possibleMoves.Add(Move.Attack(agent.RepresentingCard));
                }
            }
            else
            {
                foreach (Agent agent in enemyPlayer.Agents)
                {
                    possibleMoves.Add(Move.Attack(agent.RepresentingCard));
                }
            }
        }
        if (currentPlayer.CoinsAmount > 0)
        {
            foreach (Card card in _boardManager.Tavern.GetAffordableCards(currentPlayer.CoinsAmount))
            {
                possibleMoves.Add(Move.BuyCard(card));
            }
        }
            
        // TODO: Check why this is unused.
        List<Card> usedCards = currentPlayer.Played.Concat(currentPlayer.CooldownPile).ToList();
        if (currentPlayer.PatronCalls > 0)
        {
            foreach (var patron in _boardManager.Patrons)
            {
                if (patron.CanPatronBeActivated(currentPlayer, enemyPlayer))
                {
                    possibleMoves.Add(Move.CallPatron(patron.PatronID));
                }
            }
        }

        possibleMoves.Add(Move.EndTurn());

        return possibleMoves;
    }

    public bool IsMoveLegal(Move playerMove)
    {
        List<Move> possibleMoves = GetListOfPossibleMoves(); // might be expensive

        return possibleMoves.Contains(playerMove);
    }

    // lack of general method that parse Move and does stuff

    public void EndTurn()
    {
        _boardManager.EndTurn();
    }

    /// <summary>
    /// Returns ID or player who won the game. If game is still going it returns <c>PlayerEnum.NO_PLAYER_SELECTED</c>
    /// </summary>
    public EndGameState? CheckWinner()
    {
        return _boardManager.CheckAndGetWinner();
    }
}