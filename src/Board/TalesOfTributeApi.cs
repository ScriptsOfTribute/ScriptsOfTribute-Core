using System.Collections.Generic;

namespace TalesOfTribute
{
    class TalesOfTributeApi
    {

        private BoardManager _boardManager;

        public TalesOfTributeApi(BoardManager boardManager)
        {
            _boardManager = boardManager;
        }

        public int GetNumberOfCardsLeftInCooldownPile(int playerId)
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

        public int GetNumberOfCardsLeftInDrawPile(int playerId)
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

        public int GetNumberOfCardsLeftInHand(int playerId)
        {
            // propably only for active player
            return _boardManager.CurrentPlayer.Hand.Count;
        }

        public int GetNumberOfAgents(int playerId)
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

        public int GetNumberOfActiveAgents(int playerId)
        {
            // active - not used in turn - probably also only for active player
            return _boardManager.CurrentPlayer.Agents.FindAll(agent => !agent.Activated).Count;
        }

        public int GetHPOfAgent(Card agent)
        {
            return agent.CurrentHP;
        }

        // all to state of objects related

        public List<Card> GetHand(int playerId)
        {
            // only current player
            return _boardManager.CurrentPlayer.Hand;
        }

        public List<Card> GetPlayedCards(int playerId)
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

        public List<Card> GetDrawPile(int playerId)
        {
            // only current player
            return _boardManager.CurrentPlayer.DrawPile;
        }

        public List<Card> GetCooldownPile(int playerId)
        {
            // only current player
            return _boardManager.CurrentPlayer.CooldownPile;
        }

        public List<Card> GetTawern()
        {
            return _boardManager._tavern;
        }

        public List<Card> GetAffordableCardsInTawern(int playerId)
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

        public List<Card> GetListOfAgents(int playerId)
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

        public List<Card> GetListOfActiveAgents(int playerId)
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

        public void ActivateAgent(Card agent)
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
                return new Failure("Picked agent has been already activated in your turn");
            }
        }

        // maybe most of this function should go to BoardManager
        public void AttackAgent(Card agent)
        {
            if (_boardManager.EnemyPlayer.Agents.Contains(agent))
            {
                int attact_value = Math.Min(agent.CurrentHP, _boardManager.CurrentPlayer.PowerAmount);
                CurrentPlayer.PowerAmount -= attact_value;
                agent.CurrentHP -= attact_value;
                if (agent.CurrentHP <= 0)
                {
                    agent.CurrentHP = agent.HP;
                    _boardManager.EnemyPlayer.Agents.Remove(agent);
                    _boardManager.EnemyPlayer.CooldownPile.Add(agent);
                }
            }
            else
            {
                return new Failure("Can't attack your own agents");
            }
        }

        // Patron related

        // we need that?
        public Patron SelectPatron(PatronId patronID)
        {
            throw new NotImplementedException();
        }

        public void PatronActivation(PatronId patronID)
        {
            // only for active player
            _boardManager.PatronCall(patronID, _boardManager.CurrentPlayer, _boardManager.EnemyPlayer);
        }

        public int GetLevelOfFavoritism(int playerId, PatronId patronID)
        {
            int idx = _boardManager._patrons.IndexOf(patron => patron.PatronID == patronID);
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


        public Dictionary GetAllLevelsOfFavoritism(int playerId)
        {

            Dictionary<int, int> levelOfFavoritism = new Dictionary<int, int>();
            foreach (patron in _boardManager._patrons)
            {
                if (patron.favoredPlayer == playerId)
                {
                    levelOfFavoritism.Add(patron.PatronID, 1);
                }
                else if (patron.favoredPlayer == PlayerEnum.NO_PLAYER_SELECTED)
                {
                    levelOfFavoritism.Add(patron.PatronID, 0);
                }
                else
                {
                    levelOfFavoritism.Add(patron.PatronID, -1);
                }
            }
            return levelOfFavoritism;
        }

        // cards related

        // Implemented in BoardManager - call it from there?
        public Card BuyCard(int cardInstanceId)
        {
            throw new NotImplementedException();
        }

        public Card PlayCard(int cardInstanceId)
        {
            throw new NotImplementedException();
        }

        // do we need that?
        public Card ChooseCard(int cardInstanceId)
        {
            throw new NotImplementedException();
        }

        //others

        public List<Move> GetListOfPossibleMoves()
        {
            List<Move> possibleMoves = new List<Move>();
            Player currentPlayer = _boardManager.CurrentPlayer;
            Player enemyPlayer = _boardManager.EnemyPlayer;

            foreach (Card card in currentPlayer.Hand)
            {
                possibleMoves.Add(new Move(Command.PLAY_CARD, card.Id));
            }

            foreach (Card agent in currentPlayer.Agents)
            {
                if (!agent.Activated)
                {
                    possibleMoves.Add(new Move(Command.PLAY_CARD, agent.Id));
                }
            }

            List<Card> tauntAgents = enemyPlayer.Agents.FindAll(agent => agent.Taunt);
            if (currentPlayer.PowerAmount > 0)
            {
                if (tauntAgents.Any())
                {
                    foreach (Card agent in tauntAgents)
                    {
                        possibleMoves.Add(new Move(Command.ATTACK, agent.Id));
                    }
                }
                else
                {
                    foreach (Card agent in enemyPlayer.Agents)
                    {
                        possibleMoves.Add(new Move(Command.ATTACK, agent.Id));
                    }
                }
            }
            if (currentPlayer.CoinsAmount > 0)
            {
                foreach (Card card in _boardManager._tavern.GetAffordableCards(currentPlayer.CoinsAmount))
                {
                    possibleMoves.Add(new Move(Command.BUY_CARD, card.Id));
                }
            }

            List<Card> usedCards = currentPlayer.Played.Concat(currentPlayer.CooldownPile).ToList();
            if (currentPlayer.patronCalls > 0)
            {

                foreach (var patron in _boardManager.Patrons)
                {
                    if (patron.CanPatronBeActivated(currentPlayer, enemyPlayer))
                    {

                        possibleMoves.Add(new Move(Command.PATRON, patron.PatronID))
                    }
                }
            }

            possibleMoves.Add(new Move(Command.END_TURN));

            return possibleMoves;
        }

        public bool IsMoveLegal(Move playerMove)
        {

            List<Move> possibleMoves = GetListOfPossibleMoves(boardManager);

            return possibleMoves.Contains(playerMove);
        }

        public void EndTurn()
        {
            return _boardManager.EndTurn();
        }
    }
}