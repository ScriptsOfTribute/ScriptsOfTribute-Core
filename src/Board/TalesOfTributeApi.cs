using System.Collections.Generic;

namespace TalesOfTribute
{
    public class TalesOfTributeApi
    {

        private BoardManager _boardManager;

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
        public BoardSerializer GetSerializer()
        {
            return _boardManager.SerializeBoard();
        }

        public PlayerSerializer GetPlayersScores()
        {
            return _boardManager.GetScores();
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
        /// Get currently avalaible cards from tavern
        /// </summary>
        public List<Card> GetTavern()
        {
            return _boardManager.GetAvailableTavernCards();
        }

        /// <summary>
        /// Get cards from tavern that player with playerId can buy
        /// </summary>
        public List<Card> GetAffordableCardsInTawern(PlayerEnum playerId)
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
        public List<Card> GetAgents(PlayerEnum playerId)
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
        public List<Card> GetAgents()
        {
            return _boardManager.CurrentPlayer.Agents;
        }

        /// <summary>
        /// Get list of agents currently on board for player with playerId
        /// that are activated
        /// </summary>
        public List<Card> GetActiveAgents(PlayerEnum playerId)
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

        public List<Card> GetActiveAgents()
        {
            return _boardManager.CurrentPlayer.Agents.FindAll(agent => !agent.Activated);
        }

        //public ExecutionChain ActivateAgent(Card agent)
        //{
        //    WE DONT HANDLE IT IN BOARD MANAGER YET
        //    /* 
        //    - only active player can activate agent
        //    - it can activate player agent, not opponent
        //    - also every agent takes diffrent things to activate - I belive that 
        //    it's on us to check if it can be activated and takes good amount of Power/Coins etc
        //    from active player */
        //    if (!agent.Activated && _boardManager.CurrentPlayer.Agents.Contains(agent))
        //    {
        //        
        //    }
        //    else
        //    {
        //        throw new Exception("Picked agent has been already activated in your turn"); // return Failure
        //    }
        //}


        //public void AttackAgent(Card agent)
        //{
        //      ALL OF THIS TO BOARD MANAGER
        //    /*
        //    if (_boardManager.EnemyPlayer.Agents.Contains(agent))
        //    {
        //        int attackValue = Math.Min(agent.CurrentHP, _boardManager.CurrentPlayer.PowerAmount);
        //        _boardManager.CurrentPlayer.PowerAmount -= attackValue;
        //        agent.CurrentHP -= attackValue;
        //        if (agent.CurrentHP <= 0)
        //        {
        //            agent.CurrentHP = agent.HP;
        //            _boardManager.EnemyPlayer.Agents.Remove(agent);
        //            _boardManager.EnemyPlayer.CooldownPile.Add(agent);
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("Can't attack your own agents");
        //    }
        //    */
        //    throw new NotImplementedException();
        //}

        // Patron related

        /// <summary>
        /// Activate Patron with patronId. Only CurrentPlayer can activate patron
        /// </summary>
        public PlayResult PatronActivation(PatronId patronId)
        {
            // only for active player
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


        //public Dictionary<int, int> GetAllLevelsOfFavoritism(PlayerEnum playerId) // pointless imo
        //{
        //    Dictionary<int, int> levelOfFavoritism = new Dictionary<int, int>();
        //    foreach (var patron in _boardManager.Patrons)
        //    {
        //        if (patron.FavoredPlayer == playerId)
        //        {
        //            levelOfFavoritism.Add((int)patron.PatronID, 1);
        //        }
        //        else if (patron.FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
        //        {
        //            levelOfFavoritism.Add((int)patron.PatronID, 0);
        //        }
        //        else
        //        {
        //            levelOfFavoritism.Add((int)patron.PatronID, -1);
        //        }
        //    }
        //    return levelOfFavoritism;
        //}

        // cards related

        /// <summary>
        /// Buys card <c>card</c> in tavern for CurrentPlayer.
        /// Checks if CurrentPlayer has enough Coin and if no choice is pending.
        /// </summary>
        public ExecutionChain BuyCard(Card card)
        {
            return _boardManager.BuyCard(card);
        }

        /// <summary>
        /// Plays card <c>card</c> from hand for CurrentPlayer
        /// Checks if CurrentPlayer has this card in hand and if no choice is pending.
        /// </summary>
        public ExecutionChain PlayCard(Card card)
        {
            return _boardManager.PlayCard(card);
        }

        /// <summary>
        /// Draws 5 cards for CurrentPlayer
        /// </summary>
        public void DrawCards()
        {
            _boardManager.DrawCards();
        }

        //others

        public List<Move> GetListOfPossibleMoves()
        {
            List<Move> possibleMoves = new List<Move>();
            Player currentPlayer = _boardManager.CurrentPlayer;
            Player enemyPlayer = _boardManager.EnemyPlayer;

            foreach (Card card in currentPlayer.Hand)
            {
                possibleMoves.Add(new Move(CommandEnum.PLAY_CARD, (int)card.Id));
            }

            foreach (Card agent in currentPlayer.Agents)
            {
                if (!agent.Activated)
                {
                    possibleMoves.Add(new Move(CommandEnum.PLAY_CARD, (int)agent.Id));
                }
            }

            List<Card> tauntAgents = enemyPlayer.Agents.FindAll(agent => agent.Taunt);
            if (currentPlayer.PowerAmount > 0)
            {
                if (tauntAgents.Any())
                {
                    foreach (Card agent in tauntAgents)
                    {
                        possibleMoves.Add(new Move(CommandEnum.ATTACK, (int)agent.Id));
                    }
                }
                else
                {
                    foreach (Card agent in enemyPlayer.Agents)
                    {
                        possibleMoves.Add(new Move(CommandEnum.ATTACK, (int)agent.Id));
                    }
                }
            }
            if (currentPlayer.CoinsAmount > 0)
            {
                foreach (Card card in _boardManager.Tavern.GetAffordableCards(currentPlayer.CoinsAmount))
                {
                    possibleMoves.Add(new Move(CommandEnum.BUY_CARD, (int)card.Id));
                }
            }

            List<Card> usedCards = currentPlayer.Played.Concat(currentPlayer.CooldownPile).ToList();
            if (currentPlayer.PatronCalls > 0)
            {

                foreach (var patron in _boardManager.Patrons)
                {
                    if (patron.CanPatronBeActivated(currentPlayer, enemyPlayer))
                    {
                        possibleMoves.Add(new Move(CommandEnum.PATRON, (int)patron.PatronID));
                    }
                }
            }

            possibleMoves.Add(new Move(CommandEnum.END_TURN));

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
        public PlayerEnum CheckWinner()
        {
            return _boardManager.CheckAndGetWinner();
        }
    }
}
