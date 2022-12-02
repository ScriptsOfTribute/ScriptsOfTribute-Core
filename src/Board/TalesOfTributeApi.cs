using System.Collections.Generic;

namespace TalesOfTribute
{
    public class TalesOfTributeApi
    {

        private BoardManager _boardManager;

        public TalesOfTributeApi(BoardManager boardManager)
        {
            // what is the use case of this??
            _boardManager = boardManager;
        }

        public BoardSerializer GetSerializer()
        {
            return _boardManager.SerializeBoard();
        }

        public PlayerSerializer GetPlayersScores()
        {
            return _boardManager.GetScores();
        }

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
        }

        public int GetNumberOfCardsLeftInCooldownPile(PlayerEnum playerId)
        {
            if (playerId == _boardManager.CurrentPlayer.ID)
            {
                return _boardManager.CurrentPlayer.CooldownPile.Count;
            }
            else
            {
                return _boardManager.EnemyPlayer.CooldownPile.Count;
            }
        }

        public int GetNumberOfCardsLeftInDrawPile(PlayerEnum playerId)
        {
            if (playerId == _boardManager.CurrentPlayer.ID)
            {
                return _boardManager.CurrentPlayer.DrawPile.Count;
            }
            else
            {
                return _boardManager.EnemyPlayer.DrawPile.Count;
            }
        }

        public int GetNumberOfCardsLeftInHand()
        {
            // probably only for active player
            return _boardManager.CurrentPlayer.Hand.Count;
        }

        public int GetHPOfAgent(Card agent)
        {
            //return agent.CurrentHP;
            throw new NotImplementedException();
        }

        // all to state of objects related

        public List<Card> GetHand()
        {
            // only current player
            return _boardManager.CurrentPlayer.Hand;
        }

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

        public List<Card> GetDrawPile()
        {
            // only current player
            return _boardManager.CurrentPlayer.DrawPile;
        }

        public List<Card> GetCooldownPile()
        {
            // only current player
            return _boardManager.CurrentPlayer.CooldownPile;
        }

        public List<Card> GetTavern()
        {
            return _boardManager.GetAvailableTavernCards();
        }

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

        public List<Card> GetListOfAgents(PlayerEnum playerId)
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

        public List<Card> GetListOfActiveAgents(PlayerEnum playerId)
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
        
        
        //public void AttackAgent(Card agent) // yes, to BM
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

        public PlayResult PatronActivation(PatronId patronId)
        {
            // only for active player
            return _boardManager.PatronCall(patronId);
        }

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

        // Implemented in BoardManager - call it from there? //No
        public ExecutionChain BuyCard(Card card)
        {
            return _boardManager.BuyCard(card);
        }

        public ExecutionChain PlayCard(Card card)
        {
            return _boardManager.PlayCard(card);
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
    }
}
