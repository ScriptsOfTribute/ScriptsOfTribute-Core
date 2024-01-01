using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using System.Diagnostics;
using System.Text;
using ScriptsOfTribute.Board.CardAction;

namespace Bots;

public class RandomSimulationBot : AI
{
    private int patronFavour = 50;
    private int patronNeutral = 10;
    private int patronUnfavour = -50;
    private int coinsValue = 1;
    private int powerValue = 40;
    private int prestigeValue = 50;
    private int agentOnBoardValue = 30;
    private int hpValue = 3;
    private int opponentAgentsPenaltyValue = 40;
    private int potentialComboValue = 3;
    private int cardValue = 10;
    private int penaltyForHighTierInTavern = 2;
    private int numberOfDrawsValue = 10;
    private int enemyPotentialComboPenalty = 1;
    private List<Move> selectedTurnPlayout = new List<Move>();
    private bool newGenarate = true;
    private StringBuilder log = new StringBuilder();
    //private string logPath = "log.txt";
    private int totalNumberOfSimulationInGame = 0;
    private int totalNumberOfSimulationInTurn = 0;
    private bool startOfGame = true;
    private PlayerEnum myID;
    private string patrons;
    private string patronLogPath = "patronsRandomSimulationBot.txt";
    private Apriori apriori = new Apriori();
    private int support = 4;
    private double confidence = 0.3;
    private int allCompletedActions;
    TimeSpan usedTimeInTurn = TimeSpan.FromSeconds(0);
    TimeSpan timeForMoveComputation = TimeSpan.FromSeconds(0.5);
    TimeSpan TurnTimeout = TimeSpan.FromSeconds(30);
    private readonly SeededRandom rng = new(123);

    public int[] GetGenotype()
    {
        return new int[] { patronFavour, patronNeutral, patronNeutral, coinsValue, powerValue, prestigeValue, agentOnBoardValue, hpValue, opponentAgentsPenaltyValue, potentialComboValue, cardValue, penaltyForHighTierInTavern, numberOfDrawsValue, enemyPotentialComboPenalty };
    }

    public void SetGenotype(int[] values)
    {
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
        enemyPotentialComboPenalty = values[13];
    }

    private int CountNumberOfDrawsInTurn(List<CompletedAction> startOfTurnCompletedActions, List<CompletedAction> completedActions)
    {
        int numberOfLastActions = completedActions.Count - startOfTurnCompletedActions.Count;
        List<CompletedAction> lastCompltedActions = completedActions.TakeLast(numberOfLastActions).ToList();
        int counter = 0;
        foreach (CompletedAction action in lastCompltedActions)
        {
            if (action.Type == CompletedActionType.DRAW)
            {
                counter += 1;
            }
        }
        return counter;
    }

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        //PatronId? selectedPatron = apriori.AprioriBestChoice(availablePatrons, patronLogPath, support, confidence);
        //return selectedPatron ?? availablePatrons.PickRandom(Rng);
        return availablePatrons.PickRandom(rng);
    }

    private int BoardStateHeuristicValueEndTurn(SeededGameState gameState, List<CompletedAction> startOfturnCompletedActions)
    {
        int finalValue = 0;
        int enemyPatronFavour = 0;
        foreach (KeyValuePair<PatronId, PlayerEnum> entry in gameState.PatronStates.All)
        {
            if (entry.Key == PatronId.TREASURY)
            {
                continue;
            }
            if (entry.Value == gameState.CurrentPlayer.PlayerID)
            {
                finalValue += patronFavour;
            }
            else if (entry.Value == PlayerEnum.NO_PLAYER_SELECTED)
            {
                finalValue += patronNeutral;
            }
            else
            {
                finalValue += patronUnfavour;
                enemyPatronFavour += 1;
            }
        }
        if (enemyPatronFavour >= 2)
        {
            finalValue -= 100;
        }
        if (gameState.EnemyPlayer.Prestige >= 20)
        {
            finalValue += gameState.CurrentPlayer.Power * powerValue;
            finalValue += gameState.CurrentPlayer.Prestige * prestigeValue;
        }

        if (gameState.CurrentPlayer.Prestige < 30)
        {
            TierEnum tier = TierEnum.UNKNOWN;
            foreach (SerializedAgent agent in gameState.CurrentPlayer.Agents)
            {
                tier = CardTierList.GetCardTier(agent.RepresentingCard.Name);
                finalValue += agentOnBoardValue * (int)tier + agent.CurrentHp * hpValue;
            }

            foreach (SerializedAgent agent in gameState.EnemyPlayer.Agents)
            {
                tier = CardTierList.GetCardTier(agent.RepresentingCard.Name);
                finalValue -= agentOnBoardValue * (int)tier + agent.CurrentHp * hpValue + opponentAgentsPenaltyValue;
            }

            List<UniqueCard> allCards = gameState.CurrentPlayer.Hand.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile.Concat(gameState.CurrentPlayer.DrawPile))).ToList();
            Dictionary<PatronId, int> potentialComboNumber = new Dictionary<PatronId, int>();
            List<UniqueCard> allCardsEnemy = gameState.EnemyPlayer.Hand.Concat(gameState.EnemyPlayer.DrawPile).Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile)).ToList();
            Dictionary<PatronId, int> potentialComboNumberEnemy = new Dictionary<PatronId, int>();

            foreach (Card card in allCards)
            {
                finalValue += (int)tier * cardValue;
                if (card.Deck != PatronId.TREASURY)
                {
                    if (potentialComboNumber.ContainsKey(card.Deck))
                    {
                        potentialComboNumber[card.Deck] += 1;
                    }
                    else
                    {
                        potentialComboNumber[card.Deck] = 1;
                    }
                }
            }

            foreach (Card card in allCardsEnemy)
            {
                if (card.Deck != PatronId.TREASURY)
                {
                    if (potentialComboNumberEnemy.ContainsKey(card.Deck))
                    {
                        potentialComboNumberEnemy[card.Deck] += 1;
                    }
                    else
                    {
                        potentialComboNumberEnemy[card.Deck] = 1;
                    }
                }
            }

            foreach (KeyValuePair<PatronId, int> entry in potentialComboNumber)
            {
                finalValue += (int)Math.Pow(entry.Value, potentialComboValue);
            }
            foreach (Card card in gameState.TavernAvailableCards)
            {
                tier = CardTierList.GetCardTier(card.Name);
                finalValue -= penaltyForHighTierInTavern * (int)tier;
                /*
                if (potentialComboNumberEnemy.ContainsKey(card.Deck) && (potentialComboNumberEnemy[card.Deck]>4) && (tier > TierEnum.B)){
                    finalValue -= enemyPotentialComboPenalty*(int)tier;
                }
                */
            }

            finalValue += CountNumberOfDrawsInTurn(startOfturnCompletedActions, gameState.CompletedActions) * numberOfDrawsValue;
        }
        return finalValue;
    }

    private List<Move> NotEndTurnPossibleMoves(List<Move> possibleMoves)
    {
        return possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
    }

    private (List<Move>, SeededGameState) GenerateRandomTurnMoves(GameState gameState, List<Move> possibleMoves)
    {
        List<Move> notEndTurnPossibleMoves = NotEndTurnPossibleMoves(possibleMoves);
        List<Move> movesOrder = new List<Move>();
        var seededGameState = gameState.ToSeededGameState(123);
        while (notEndTurnPossibleMoves.Count != 0)
        {
            Move chosenMove;
            if ((seededGameState.BoardState == BoardState.NORMAL) && (Extensions.RandomK(0, 10000, rng) == 0))
            {
                chosenMove = Move.EndTurn();
            }
            else
            {
                chosenMove = notEndTurnPossibleMoves.PickRandom(rng);
            }
            movesOrder.Add(chosenMove);

            if (chosenMove.Command == CommandEnum.END_TURN)
            {
                return (movesOrder, seededGameState);
            }

            (var newGameState, List<Move> newPossibleMoves) = seededGameState.ApplyMove(chosenMove);
            if (newGameState.GameEndState?.Winner == myID)
            {
                return (movesOrder, newGameState);
            }
            notEndTurnPossibleMoves = NotEndTurnPossibleMoves(newPossibleMoves);
            seededGameState = newGameState;
        }
        return (movesOrder, seededGameState);
    }

    private List<Move> GenerateKTurnGamesAndSelectBest(GameState? gameState, List<Move> possibleMoves)
    {
        List<Move> bestPlayout = new List<Move>();
        int highestHeuristicValue = int.MinValue;
        int heuristicValue = 0;
        int howManySimulation = 0;
        Stopwatch s = new Stopwatch();
        s.Start();

        while (s.Elapsed < timeForMoveComputation)
        {
            (List<Move> generatedPlayout, var endTurnBoard) = GenerateRandomTurnMoves(gameState, possibleMoves);
            if (endTurnBoard.GameEndState?.Winner == myID)
            {
                return generatedPlayout;
            }

            heuristicValue = BoardStateHeuristicValueEndTurn(endTurnBoard, gameState.CompletedActions);

            if (highestHeuristicValue < heuristicValue)
            {
                bestPlayout = generatedPlayout;
                highestHeuristicValue = heuristicValue;
            }
            howManySimulation += 1;
        }
        totalNumberOfSimulationInGame += howManySimulation;

        return bestPlayout;
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
    {
        if (startOfGame)
        {
            myID = gameState.CurrentPlayer.PlayerID;
            allCompletedActions = gameState.CompletedActions.Count;
            patrons = string.Join(",", gameState.Patrons.FindAll(x => x != PatronId.TREASURY).Select(n => n.ToString()).ToArray());
            startOfGame = false;
        }
        int numberOfLastActions = gameState.CompletedActions.Count - allCompletedActions;
        List<CompletedAction> lastCompltedActions = gameState.CompletedActions.TakeLast(numberOfLastActions).ToList();
        allCompletedActions = gameState.CompletedActions.Count;
        foreach (CompletedAction action in lastCompltedActions)
        {
            if (action.Type == CompletedActionType.DRAW)
            {
                newGenarate = true;
            }
        }

        if (newGenarate)
        {
            if (timeForMoveComputation + usedTimeInTurn >= TurnTimeout)
            {
                selectedTurnPlayout = new List<Move> { possibleMoves.PickRandom(rng) };
            }
            else
            {
                selectedTurnPlayout = GenerateKTurnGamesAndSelectBest(gameState, possibleMoves);
                usedTimeInTurn += timeForMoveComputation;
            }
            newGenarate = false;
        }

        if (selectedTurnPlayout.Count != 0)
        {
            Move move = selectedTurnPlayout[0];
            selectedTurnPlayout.RemoveAt(0);
            if (!possibleMoves.Contains(move))
            {
                selectedTurnPlayout = GenerateKTurnGamesAndSelectBest(gameState, possibleMoves);
                newGenarate = false;
                move = selectedTurnPlayout[0];
                selectedTurnPlayout.RemoveAt(0);
            }
            if (move.Command == CommandEnum.MAKE_CHOICE)
            {
                newGenarate = true;
            }
            return move;
        }
        else
        {
            newGenarate = true;
            totalNumberOfSimulationInTurn = 0;
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            return Move.EndTurn();
        }
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        if (state.Winner == myID)
        {
            File.AppendAllText(patronLogPath, patrons + System.Environment.NewLine);
        }
    }
}
