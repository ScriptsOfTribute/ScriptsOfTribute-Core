using System.Collections.Generic;

namespace TalesOfTribute {
  class TalesOfTributeApi {
<<<<<<< HEAD

    private BoardManager _boardManager;

    public TalesOfTributeApi(BoardManager boardManager) {
      _boardManager = boardManager;
    }
=======
>>>>>>> 48909ce349d75428fba942f405f8beb5cbcd9e11
    //all numbers related
    /* Probably its better to have two function:
    GetMyAmountOfCoins and GetOpponentAmountOfCoins but it doubles numbers of functions */

    public int GetAmountOfCoins(int playerId) {
      throw new NotImplementedException();
    }

    public int GetAmountOfPower(int playerId) {
      throw new NotImplementedException();
    }

    public int GetAmountOfPrestige(int playerId) {
      throw new NotImplementedException();
    }

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

    public int ChangeHPOfAgent(int agentId, int hpDelta) {
      throw new NotImplementedException();
    }

    public int ChangeAmountOfCoins(int playerId, int coinsDelta) {
      throw new NotImplementedException();
    }

    public int ChangeAmountOfPower(int playerId, int powerDelta) {
      throw new NotImplementedException();
    }

    public int ChangeAmountOfPrestige(int playerId, int prestigeDelta) {
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

<<<<<<< HEAD
    public List < Card > GetListOfAgents(int playerId) {
      throw new NotImplementedException();
    }

    public List < Card > GetListOfActiveAgents(int playerId) {
=======
    public List<Card> GetListOfAgents(int playerId) {
      throw new NotImplementedException();
    }

    public List<Card> GetListOfActiveAgents(int playerId) {
>>>>>>> 48909ce349d75428fba942f405f8beb5cbcd9e11
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

    public Card DrawCard() {
      throw new NotImplementedException();
    }

<<<<<<< HEAD
    public Card BuyCard(int cardInstanceId) {
=======
    public Card BuyCard() {
>>>>>>> 48909ce349d75428fba942f405f8beb5cbcd9e11
      throw new NotImplementedException();
    }

    public Card DiscardCard(int cardInstanceId) {
      throw new NotImplementedException();
    }

    public void Knockout(int agentId) {
      throw new NotImplementedException();
    }

    public Card RemoveCard(int cardInstanceId) {
      throw new NotImplementedException();
    }

    public Card RemoveCardFromTavern(int cardInstanceId) {
      throw new NotImplementedException();
    }

    public Card DestroyCard(int cardInstanceId) {
      throw new NotImplementedException();
    }

    public Card ReturnTop(int amountOfCards) {
      throw new NotImplementedException();
    }

    public Card AddCard(int cardInstanceId) {
      throw new NotImplementedException();
    }

    public Card CreateCard(int cardId) {
      throw new NotImplementedException();
    }

    public Card AcquireCard(int cardInstanceId) {
      // eg Oathman
      throw new NotImplementedException();
    }

    public List < Card > Toss() {
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

    public Player ChoosePlayer(int playerId) {
      throw new NotImplementedException();
    }

    public Card ChooseCard(int cardInstanceId) {
      throw new NotImplementedException();
    }

    //others

<<<<<<< HEAD
    public List < Move > GetListOfPossibleMoves() {
      List < Move > possibleMoves = new List < Move > ();
      Player currentPlayer = _boardManager.CurrentPlayer;
      Player enemyPlayer = _boardManager.EnemyPlayer;

      foreach(Card card in currentPlayer.Hand) {
        possibleMoves.Add(new Move(Command.PLAY_CARD, card.Id));
      }

      foreach(Card agent in currentPlayer.Agents) {
        if (!agent.Activated) {
=======
    public Move FromStringToMove(string move){
        string[] splittedMove = move.Split(' ');

        if (splittedMove.Length !=2){
            throw new InvalidOperationException();
        }

        if (!Command.ValidateStringCommand(splittedMove[0])){
            throw new InvalidOperationException();
        }

        try{
            int value = Int32.Parse(splittedMove[1]);
            return new Move(splittedMove[0], value);
        }
        catch (FormatException e){
            throw new InvalidOperationException();
        }
    }

    public List<Move> GetListOfPossibleMoves(BoardManager boardManager) {
      List<Move> possibleMoves = new List<Move>();
      var current_player = boardManager.Players[(int)boardManager.CurrentPlayer];
      var opponent = boardManager.Players[1 - (int)boardManager.CurrentPlayer];

      foreach (Card card in current_player.Hand){
        possibleMoves.Add(new Move(Command.PLAY_CARD, card.Id));
      }

      foreach (Card agent in current_player.Agents){
        if (!agent.Activated){
>>>>>>> 48909ce349d75428fba942f405f8beb5cbcd9e11
          possibleMoves.Add(new Move(Command.PLAY_CARD, agent.Id));
        }
      }

<<<<<<< HEAD
      List < Card > tauntAgents = enemyPlayer.Agents.FindAll(agent => agent.Taunt);
      if (currentPlayer.PowerAmount > 0) {
        if (tauntAgents.Any()) {
          foreach(Card agent in tauntAgents) {
            possibleMoves.Add(new Move(Command.ATTACK, agent.Id));
          }
        } else {
          foreach(Card agent in enemyPlayer.Agents) {
=======
      List<Card> tauntAgents = opponent.Agents.FindAll(agent => agent.Taunt);
      if (current_player.PowerAmount>0){
        if (tauntAgents.Any()){
          foreach (Card agent in tauntAgents){
            possibleMoves.Add(new Move(Command.ATTACK, agent.Id));
          }
        }
        else{
          foreach (Card agent in opponent.Agents){
>>>>>>> 48909ce349d75428fba942f405f8beb5cbcd9e11
            possibleMoves.Add(new Move(Command.ATTACK, agent.Id));
          }
        }
      }
<<<<<<< HEAD
      if (currentPlayer.CoinsAmount > 0) {
        foreach(Card card in _boardManager._tavern.GetAffordableCards(currentPlayer.CoinsAmount)) {
          possibleMoves.Add(new Move(Command.BUY_CARD, card.Id));
        }
      }

      List < Card > usedCards = currentPlayer.Played.Concat(currentPlayer.CooldownPile).ToList();
      if (currentPlayer.patronCalls > 0) {

        foreach(var patron in boardManager.Patrons) {
          if (patron.CanPatronBeActivated(currentPlayer, enemyPlayer)) {

            switch (patron.PatronID) {

            case PatronId.TREASURY:
              List < Card > usedCards = currentPlayer.Played.Concat(currentPlayer.CooldownPile).ToList();
              foreach(Card card in usedCards) {
                possibleMoves.Add(new Move(Command.TREASURY, card.Id));
              }
              break;

            case PatronId.DUKE_OF_CROWS:
              possibleMoves.Add(new Move(Command.DUKE_OF_CROWS));
              break;

            case PatronId.RAJHIN:
              possibleMoves.Add(new Move(Command.RAJHIN));
              break;

            case PatronId.ORGNUM:
              possibleMoves.Add(new Move(Command.ORGNUM));
              break;

            case PatronId.PSIJIC:
              if (tauntAgents.Any()) {
                foreach(Card agent in tauntAgents) {
                  possibleMoves.Add(new Move(Command.PSIJIC, agent.Id));
                }
              } else {
                foreach(Card agent in enemyPlayer.Agents) {
                  possibleMoves.Add(new Move(Command.PSIJIC, agent.Id));
                }
              }
              break;

            case PatronId.HLAALU:
              // not sure it will be all card that player own or only all without drawpile
              List < Card > cardsWithCost = currentPlayer.GetAllPlayersCards().FindAll(card => card.Cost >= 1);
              foreach(var card in cardsWithCost) {
                possibleMoves.Add(new Move(Command.HLAALU, card.Id));
              }
              breal;

            case PatronId.ANSEI:
              possibleMoves.Add(new Move(Command.ANSEI));
              break;

            case PatronId.RED_EAGLE:
              possibleMoves.Add(new Move(Command.RED_EAGLE));

            case PatronId.PELIN:
              List < Card > agentsInCooldownPile = currentPlayer.CooldownPile.FindAll(card => card.Type == AGENT);
              foreach(Card agent in agentsInCooldownPile) {
                possibleMoves.Add(new Move(Command.PELIN, agent.Id));
              }
            }
          }
=======
      if (current_player.CoinsAmount>0){
        foreach (Card card in Tawern){
          if (card.Cost<=current_player.CoinsAmount){
            possibleMoves.Add(new Move(Command.BUY_CARD, card.Id));
          }
        }
      }

      List<Card> usedCards = current_player.Played.Concat(current_player.CooldownPile).ToList();
      if (current_player.patronCalls>0){

        if (current_player.CoinsAmount>=2){
          foreach (Card card in usedCards){
            possibleMoves.Add(new Move(Command.TREASURY, card.Id));
          }
        }

        foreach (var patron in boardManager.Patrons){
          if (patron.ID == PatronId.DUKE_OF_CROWS){
            if (current_player.CoinsAmount>0 && patron.FavoredPlayer != current_player.ID){
              possibleMoves.Add(new Move(Command.DUKE_OF_CROWS));
            }
          }

          if (current_player.PowerAmount>=2){
            if (patron.ID == PatronId.RED_EAGLE){
              possibleMoves.Add(new Move(Command.RED_EAGLE));
            }
            if (patron.ID == PatronId.ANSEI && patron.FavoredPlayer != current_player.ID){
              possibleMoves.Add(new Move(Command.ANSEI));
            }
            if (patron.ID == PatronId.PELIN){
              List<Card> agentsInCooldownPile = current_player.CooldownPile.FindAll(card => card.Type==AGENT);
              foreach (Card agent in agentsInCooldownPile){
                possibleMoves.Add(new Move(Command.PELIN, agent.Id));
              }
            }
          }

          if (current_player.CoinsAmount>=3){
            if (patron.ID == PatronId.RAJHIN){
              possibleMoves.Add(new Move(Command.RAJHIN));
            }
            if (patron.ID == PatronId.ORGNUM){
              possibleMoves.Add(new Move(Command.ORGNUM));
            }
          }

          if (patron.ID == PatronId.PSIJIC && current_player.CoinsAmount>=4){
            if (tauntAgents.Any()){
              foreach (Card agent in tauntAgents){
                possibleMoves.Add(new Move(Command.PSIJIC, agent.Id));
              }
            }
            else{
              foreach (Card agent in opponent.Agents){
                possibleMoves.Add(new Move(Command.PSIJIC, agent.Id));
              }
            }
          }

          if (patron.ID == PatronId.PSIJIC){
            // not sure it will be all card that player own or only all without drawpile
            List<Card> cardsWithCost = current_player.GetAllPlayersCards().FindAll(card => card.Cost>=1);
            foreach (var card in cardsWithCost){
              possibleMoves.Add(new Move(Command.HLAALU, card.Id));
            }
          }
>>>>>>> 48909ce349d75428fba942f405f8beb5cbcd9e11
        }
      }

      possibleMoves.Add(new Move(Command.END_TURN));

      return possibleMoves;
    }

<<<<<<< HEAD
    public bool IsMoveLegal(Move playerMove) {

      List < Move > possibleMoves = GetListOfPossibleMoves(boardManager);
=======
    public bool IsMoveLegal(string move, BoardManager boardManager) {
      
      Move playerMove = FromStringToMove(move);
      List<Move> possibleMoves = GetListOfPossibleMoves(boardManager);
>>>>>>> 48909ce349d75428fba942f405f8beb5cbcd9e11

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

<<<<<<< HEAD
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

=======
>>>>>>> 48909ce349d75428fba942f405f8beb5cbcd9e11
  }
}