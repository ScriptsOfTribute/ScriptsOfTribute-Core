using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;
using TalesOfTribute.Board.Cards;
using System.Diagnostics;
using System.Text;

namespace SimpleBots;

public class RandomHeuristicBot : AI
{
    private int patronFavour = 200;
    private int patronNeutral = 100;
    private int patronUnfavour = -200;
    private int coinsValue = 10;
    private int powerValue = 400;
    private int prestigeValue = 1000;
    private int agentOnBoardValue = 125;
    private int hpValue = 20;
    private int opponentAgentsPenaltyValue = 40;
    private int potentialComboValue = 20;
    private int cardValue = 10;
    private int penaltyForHighTierInTavern = 40;
    private int numberOfDrawsValue = 35;
    private List<Move> selectedTurnPlayout = new List<Move>();
    private bool newGenarate = true;
    private StringBuilder log = new StringBuilder();
    private string patrons;
    private string patronLogPath = "patrons.txt";
    private string logPath = "log.txt";
    private int totalNumberOfSimulationInGame = 0;
    private int totalNumberOfSimulationInTurn = 0;
    private bool startOfGame = true;
    private PlayerEnum myID;
    private Apriori apriori = new Apriori();
    private int support = 4;
    private double confidence = 0.3;
   
    public int[] GetGenotype(){
        return new int[] {patronFavour, patronNeutral, patronNeutral, coinsValue, powerValue, prestigeValue, agentOnBoardValue, hpValue, opponentAgentsPenaltyValue, potentialComboValue, cardValue, penaltyForHighTierInTavern, numberOfDrawsValue};
    }

    public void SetGenotype(int[] values){
        patronFavour = values[0];
        patronNeutral = values[1];
        patronNeutral = values[2];
        coinsValue = values[3];
        powerValue = values[4];
        prestigeValue = values[5]; 
        agentOnBoardValue = values[6];
        hpValue = values[7];
        opponentAgentsPenaltyValue = values[8];
        potentialComboValue = values[9];
        cardValue = values[10];
        penaltyForHighTierInTavern = values[11];
        numberOfDrawsValue = values[12];
    }

    private int CountNumberOfDrawsInTurn(List<CompletedAction> startOfTurnCompletedActions, List<CompletedAction> completedActions){
        int numberOfLastActions = completedActions.Count - startOfTurnCompletedActions.Count;
        List<CompletedAction> lastCompltedActions = completedActions.TakeLast(numberOfLastActions).ToList();
        int counter = 0;
        foreach (CompletedAction action in lastCompltedActions){
            if (action.Type == CompletedActionType.DRAW){
                counter += 1;
            }
        }
        return counter;
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round){
        PatronId? selectedPatron = apriori.AprioriBestChoice(availablePatrons, patronLogPath, support, confidence);
        return selectedPatron ?? availablePatrons.PickRandom(Rng);
    }

    private int BoardStateHeuristicValueEndTurn(GameState gameState, List<CompletedAction> startOfturnCompletedActions){
        int finalValue = 0;
        foreach (KeyValuePair<PatronId, PlayerEnum> entry in gameState.PatronStates.All) {
            if (entry.Value == gameState.CurrentPlayer.PlayerID) {
                finalValue += patronFavour;
            }
            else if (entry.Value == PlayerEnum.NO_PLAYER_SELECTED){
                finalValue += patronNeutral;
            }
            else{
                finalValue += patronUnfavour;
            }
        }

        finalValue += gameState.CurrentPlayer.Coins * coinsValue;
        finalValue += gameState.CurrentPlayer.Power * powerValue;
        finalValue += gameState.CurrentPlayer.Prestige * prestigeValue;
        
        int tier = -10000;
        foreach (SerializedAgent agent in gameState.CurrentPlayer.Agents){
            tier = (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
            finalValue += agentOnBoardValue * tier + agent.CurrentHp * hpValue;
        }

        foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents){
            tier = (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
            finalValue -= agentOnBoardValue * tier + agent.CurrentHp * hpValue + opponentAgentsPenaltyValue;
        }

        List<UniqueCard> allCards = gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile.Concat(gameState.CurrentPlayer.DrawPile))).ToList();
        Dictionary<PatronId, int> potentialComboNumber = new Dictionary<PatronId, int>();

        foreach(Card card in allCards){
            finalValue += (int)CardTierList.GetCardTier(card.Name) * cardValue;
            if (card.Deck != PatronId.TREASURY){
                if (potentialComboNumber.ContainsKey(card.Deck)){
                    potentialComboNumber[card.Deck] +=1;
                }
                else{
                    potentialComboNumber[card.Deck] = 1;
                }
            }
        }
        foreach (KeyValuePair<PatronId, int> entry in potentialComboNumber){
            finalValue += (int)Math.Pow(entry.Value, potentialComboValue);
        }
        foreach(Card card in gameState.TavernCards){
            finalValue -= penaltyForHighTierInTavern * (int)CardTierList.GetCardTier(card.Name);
        }

        finalValue += CountNumberOfDrawsInTurn(startOfturnCompletedActions, gameState.CompletedActions) * numberOfDrawsValue;
        return finalValue;
    }

    private List<Move> NotEndTurnPossibleMoves(List<Move> possibleMoves){
        return possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
    }

    private (List<Move>, GameState) GenerateRandomTurnMoves(GameState gameState, List<Move> possibleMoves){
        List<Move> notEndTurnPossibleMoves = NotEndTurnPossibleMoves(possibleMoves);
        List<Move> movesOrder = new List<Move>();
        while (notEndTurnPossibleMoves.Count != 0){
            Move chosenMove = notEndTurnPossibleMoves.PickRandom(Rng);
            movesOrder.Add(chosenMove);
            (gameState, List<Move> newPossibleMoves) = gameState.ApplyState(chosenMove);
            notEndTurnPossibleMoves = NotEndTurnPossibleMoves(newPossibleMoves);
        }
        return (movesOrder, gameState);
    }

    private List<Move> GenerateKTurnGamesAndSelectBest(GameState gameState, List<Move> possibleMoves){
        List<Move> bestPlayout = new List<Move>();
        int highestHeuristicValue = -100000000;
        int heuristicValue = 0;
        int howManySimulation = 0;
        Stopwatch s = new Stopwatch();
        s.Start();
        while (s.Elapsed < TimeSpan.FromSeconds(0.01)){
            (List<Move> generatedPlayout, GameState endTurnBoard)= GenerateRandomTurnMoves(gameState, possibleMoves);
            heuristicValue = BoardStateHeuristicValueEndTurn(endTurnBoard, gameState.CompletedActions);
            if (highestHeuristicValue < heuristicValue){
                bestPlayout = generatedPlayout;
                highestHeuristicValue = heuristicValue;
            }
            howManySimulation +=1;
        }
        log.Append(howManySimulation.ToString()+ System.Environment.NewLine);
        totalNumberOfSimulationInGame += howManySimulation;
        totalNumberOfSimulationInTurn += howManySimulation;
        return bestPlayout;
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfGame){
            myID = gameState.CurrentPlayer.PlayerID;
            patrons = string.Join(",", gameState.Patrons.FindAll(x => x != PatronId.TREASURY).Select(n => n.ToString()).ToArray());
            startOfGame = false;
        }
        if (newGenarate){
            selectedTurnPlayout = GenerateKTurnGamesAndSelectBest(gameState, possibleMoves);
            newGenarate = false;
        }
        if (selectedTurnPlayout.Count != 0){
            Move move = selectedTurnPlayout[0];
            selectedTurnPlayout.RemoveAt(0);
            if (!possibleMoves.Contains(move)){
                selectedTurnPlayout = GenerateKTurnGamesAndSelectBest(gameState, possibleMoves);
                newGenarate = false;
                move = selectedTurnPlayout[0];
                selectedTurnPlayout.RemoveAt(0);
            }
            if (move.Command == CommandEnum.MAKE_CHOICE){
                newGenarate = true;
            }
            return move;
        }
        else{
            newGenarate = true;
            log.Append("Total number of simulation in this turn: " + totalNumberOfSimulationInTurn.ToString()+ System.Environment.NewLine);
            totalNumberOfSimulationInTurn = 0;
            return Move.EndTurn();
        }
    }

    public override void GameEnd(EndGameState state)
    {
        if (state.Winner == myID){
            File.AppendAllText(patronLogPath, patrons + System.Environment.NewLine);
        }
        log.Append("Total number of simulation in game: " + totalNumberOfSimulationInGame.ToString()+ System.Environment.NewLine);
        File.AppendAllText(logPath, log.ToString());
        log.Clear();
    }
}
