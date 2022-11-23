using System.Collections.Generic;

namespace TalesOfTribute {
  class TalesOfTributeApi {

    private BoardManager _boardManager;

    public TalesOfTributeApi(BoardManager boardManager) {
      _boardManager = boardManager;
    }
    //all numbers related
    /* Probably its better to have two function:
    GetMyAmountOfCoins and GetOpponentAmountOfCoins but it doubles numbers of functions */

    public int GetNumberOfCardsLeftInCooldownPile(int playerId) {
      throw new NotImplementedException();
    }

    public int GetNumberOfCardsLeftInDrawPile(int playerId) {
      throw new NotImplementedException();
    }

    public int GetNumberOfCardsLeftInHand(int playerId) {
      // propably only for active player
      throw new NotImplementedException();
    }

    public int GetNumberOfAgents(int playerId) {
      throw new NotImplementedException();
    }

    public int GetNumberOfActiveAgents(int playerId) {
      throw new NotImplementedException();
    }

    public int GetHPOfAgent(int agentId) {
      throw new NotImplementedException();
    }

    // all to state of objects related

    public List < Card > GetHand(int playerId) {
      throw new NotImplementedException();
    }

    public List < Card > GetPlayedCards(int playerId) {
      throw new NotImplementedException();
    }

    public List < Card > GetDrawPile(int playerId) {
      throw new NotImplementedException();
    }

    public List < Card > GetCooldownPile(int playerId) {
      throw new NotImplementedException();
    }

    public List < Card > GetTawern() {
      throw new NotImplementedException();
    }

    public List < Card > GetAffordableCardsInTawern(int playerId) {
      throw new NotImplementedException();
    }

    // Agents related
    //I assume that we will handle Agents special

    public List < Card > GetListOfAgents(int playerId) {
      throw new NotImplementedException();
    }

    public List < Card > GetListOfActiveAgents(int playerId) {
      throw new NotImplementedException();
    }

    public void ActivateAgent(int agentId) {
      /* 
      - only active player can activate agent
      - it can activate player agent, not opponent
      - also every agent takes diffrent things to activate - I belive that 
      it's on us to check if it can be activated and takes good amount of Power/Coins etc
      from active player */
      throw new NotImplementedException();
    }

    public void CallAgent(int agentId) {
      throw new NotImplementedException();
    }

    public void AttackAgent(int agentId) {
      throw new NotImplementedException();
    }

    // Patron related

    public Patron SelectPatron(string patronName) {
      throw new NotImplementedException();
    }

    public void PatronActivation(string patronName) {
      // only for active player
      throw new NotImplementedException();
    }

    public int GetLevelOfFavoritism(int playerId, string patronName) {
      throw new NotImplementedException();
    }

    /* TODO
    public Dictionary GetAllLevelsOfFavoritism(int playerId) {
      throw new NotImplementedException();
    }
    */

    // cards related

    public Card BuyCard(int cardInstanceId) {
      throw new NotImplementedException();
    }

    // Important for future: some cards have Taunt efects and must be attacked first

    public Card PlayCard(int cardInstanceId) {
      // playing a card could have multiple effects - sometimes player need to choose more than one card
      /* We can handle it in two ways
      1) PlayCard gets a string, schema sth like "cardInstanceId ChosenId1, ChosenId2, ...END"
      we validate that move and apply effects or
      2) PlayCard always takes only cardInstanceId but it's on our side to check if move is legal and
      ask player to choose cards one by one
      3) some cards moves another cards from one pile to another (Ansei return etc)
      */
      throw new NotImplementedException();
    }

    public Card ChooseCard(int cardInstanceId) {
      throw new NotImplementedException();
    }

    //others

    public List < Move > GetListOfPossibleMoves() {
      List < Move > possibleMoves = new List < Move > ();
      Player currentPlayer = _boardManager.CurrentPlayer;
      Player enemyPlayer = _boardManager.EnemyPlayer;

      foreach(Card card in currentPlayer.Hand) {
        possibleMoves.Add(new Move(Command.PLAY_CARD, card.Id));
      }

      foreach(Card agent in currentPlayer.Agents) {
        if (!agent.Activated) {
          possibleMoves.Add(new Move(Command.PLAY_CARD, agent.Id));
        }
      }

      List < Card > tauntAgents = enemyPlayer.Agents.FindAll(agent => agent.Taunt);
      if (currentPlayer.PowerAmount > 0) {
        if (tauntAgents.Any()) {
          foreach(Card agent in tauntAgents) {
            possibleMoves.Add(new Move(Command.ATTACK, agent.Id));
          }
        } else {
          foreach(Card agent in enemyPlayer.Agents) {
            possibleMoves.Add(new Move(Command.ATTACK, agent.Id));
          }
        }
      }
      if (currentPlayer.CoinsAmount > 0) {
        foreach(Card card in _boardManager._tavern.GetAffordableCards(currentPlayer.CoinsAmount)) {
          possibleMoves.Add(new Move(Command.BUY_CARD, card.Id));
        }
      }

      List < Card > usedCards = currentPlayer.Played.Concat(currentPlayer.CooldownPile).ToList();
      if (currentPlayer.patronCalls > 0) {

        foreach(var patron in _boardManager.Patrons) {
          if (patron.CanPatronBeActivated(currentPlayer, enemyPlayer)) {

            possibleMoves.Add(new Move(Command.PATRON, patron.PatronID))
          }
        }
      }

      possibleMoves.Add(new Move(Command.END_TURN));

      return possibleMoves;
    }

    public bool IsMoveLegal(Move playerMove) {

      List < Move > possibleMoves = GetListOfPossibleMoves(boardManager);

      return possibleMoves.Contains(playerMove);
    }

    public void EndTurn() {
      //move cards from used to cooldown pile, etc
      throw new NotImplementedException();
    }

    // Better place for that function is in Manager
    public void CheckWinConditions() {
      /* 1) four patrons favours active player - easy
      2) win by prestige - harder to check - depends on another player and its next turn */
      throw new NotImplementedException();
    }

    public Move FromStringToMove(string move) {
      string[] splittedMove = move.Split(' ');

      if (splittedMove.Length != 2) {
        throw new InvalidOperationException();
      }
      try {
        int value = Int32.Parse(splittedMove[1]);
        return new Move(splittedMove[0], value);
      } catch (FormatException e) {
        throw new InvalidOperationException();
      } catch (InvalidOperationException e) {
        throw new InvalidOperationException();
      }
    }

    public void Parser(string move) {
      Move playerMove = FromStringToMove(move);

      switch (playerMove.Command) {
      case CommandEnum.GET_POSSIBLE_MOVES:
        List < Move > moves = GetListOfPossibleMoves();
        foreach(var move in moves) {
          Console.WriteLine(move.ToString());
        }
        break;

      case CommandEnum.END_TURN:
        _boardManager.EndTurn();
        break;
        //TODO rest
      default:
        break;
      }
    }

  }
}