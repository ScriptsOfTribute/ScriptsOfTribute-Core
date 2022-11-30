using System.Collections.Generic;

namespace TalesOfTribute
{
    public class TalesOfTributeApi
    {

        private BoardManager _boardManager;

        public TalesOfTributeApi(BoardManager boardManager)
        {
            _boardManager = boardManager;
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

        public int GetNumberOfAgents(PlayerEnum playerId)
        {
            if (playerId == _boardManager.CurrentPlayer.ID)
            {
                return _boardManager.CurrentPlayer.Agents.Count;
            }
            else
            {
                return _boardManager.EnemyPlayer.Agents.Count;
            }
        }

        public int GetNumberOfActiveAgents()
        {
            // active - not used in turn - probably also only for active player
            return _boardManager.CurrentPlayer.Agents.FindAll(agent => !agent.Activated).Count;
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

        public ExecutionChain ActivateAgent(Card agent)
        {
            /* 
            - only active player can activate agent
            - it can activate player agent, not opponent
            - also every agent takes diffrent things to activate - I belive that 
            it's on us to check if it can be activated and takes good amount of Power/Coins etc
            from active player */
            if (!agent.Activated && _boardManager.CurrentPlayer.Agents.Contains(agent))
            {
                return _boardManager.PlayCard(agent);
            }
            else
            {
                throw new Exception("Picked agent has been already activated in your turn");
            }
        }

        // maybe most of this function should go to BoardManager
        public void AttackAgent(Card agent)
        {
            /*
            if (_boardManager.EnemyPlayer.Agents.Contains(agent))
            {
                int attackValue = Math.Min(agent.CurrentHP, _boardManager.CurrentPlayer.PowerAmount);
                _boardManager.CurrentPlayer.PowerAmount -= attackValue;
                agent.CurrentHP -= attackValue;
                if (agent.CurrentHP <= 0)
                {
                    agent.CurrentHP = agent.HP;
                    _boardManager.EnemyPlayer.Agents.Remove(agent);
                    _boardManager.EnemyPlayer.CooldownPile.Add(agent);
                }
            }
            else
            {
                throw new Exception("Can't attack your own agents");
            }
            */
            throw new NotImplementedException();
        }

        // Patron related

        public PlayResult PatronActivation(PatronId patronID)
        {
            // only for active player
            return _boardManager.PatronCall((int)patronID, _boardManager.CurrentPlayer, _boardManager.EnemyPlayer);
        }

        public int GetLevelOfFavoritism(PlayerEnum playerId, PatronId patronID)
        {
            int idx = Array.FindIndex(_boardManager.Patrons, patron => patron.PatronID == patronID);
            PlayerEnum favoredPlayer = _boardManager.GetPatronFavorism(idx);
            if (favoredPlayer == playerId)
            {
                return 1;
            }
            else if (favoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }


        public Dictionary<int, int> GetAllLevelsOfFavoritism(PlayerEnum playerId)
        {
            Dictionary<int, int> levelOfFavoritism = new Dictionary<int, int>();
            foreach (var patron in _boardManager.Patrons)
            {
                if (patron.FavoredPlayer == playerId)
                {
                    levelOfFavoritism.Add((int)patron.PatronID, 1);
                }
                else if (patron.FavoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                {
                    levelOfFavoritism.Add((int)patron.PatronID, 0);
                }
                else
                {
                    levelOfFavoritism.Add((int)patron.PatronID, -1);
                }
            }
            return levelOfFavoritism;
        }

        // cards related

        // Implemented in BoardManager - call it from there?
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
            List<Move> possibleMoves = GetListOfPossibleMoves();

            return possibleMoves.Contains(playerMove);
        }

        public void EndTurn()
        {
            _boardManager.EndTurn();
        }
    }
}
