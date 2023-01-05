using SimpleBots;
using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;
using TalesOfTribute.Board.Cards;
using System.Diagnostics;
using System.Text;

namespace SimpleBotsTests;

public class RandomHeuristicBot : AI
{
    const int patronFavour = 200;
    const int patronNeutral = 100;
    const int patronUnfavour = -200;
    const int coinsValue = 10;
    const int powerValue = 400;
    const int prestigeValue = 1000;
    const int agentOnBoardValue = 125;
    const int hpValue = 20;
    const int opponentAgentsPenaltyValue = 40;
    const int potentialComboValue = 20;
    const int cardValue = 10;
    const int penaltyForHighTierInTavern = 40;
    private List<Move> selectedTurnPlayout = new List<Move>();
    private bool newGenarate = true;
    private StringBuilder log = new StringBuilder();
    private int totalNumberOfSimulationInGame = 0;
    private int totalNumberOfSimulationInTurn = 0;

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom();

    private int BoardStateHeuristicValueEndTurn(SerializedBoard serializedBoard){
        int finalValue = 0;
        foreach (KeyValuePair<PatronId, PlayerEnum> entry in serializedBoard.PatronStates.All) {
            if (entry.Value == serializedBoard.CurrentPlayer.PlayerID) {
                finalValue += patronFavour;
            }
            else if (entry.Value == PlayerEnum.NO_PLAYER_SELECTED){
                finalValue += patronNeutral;
            }
            else{
                finalValue += patronUnfavour;
            }
        }

        finalValue += serializedBoard.CurrentPlayer.Coins * coinsValue;
        finalValue += serializedBoard.CurrentPlayer.Power * powerValue;
        finalValue += serializedBoard.CurrentPlayer.Prestige * prestigeValue;
        
        int tier = -10000;
        foreach (SerializedAgent agent in serializedBoard.CurrentPlayer.Agents){
            tier = (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
            finalValue += agentOnBoardValue * tier + agent.CurrentHp * hpValue;
        }

        foreach (SerializedAgent agent in serializedBoard.EnemyPlayer.Agents){
            tier = (int)CardTierList.GetCardTier(agent.RepresentingCard.Name);
            finalValue -= agentOnBoardValue * tier + agent.CurrentHp * hpValue + opponentAgentsPenaltyValue;
        }

        List<UniqueCard> allCards = serializedBoard.CurrentPlayer.Hand.Concat(serializedBoard.CurrentPlayer.Played.Concat(serializedBoard.CurrentPlayer.CooldownPile.Concat(serializedBoard.CurrentPlayer.DrawPile))).ToList();
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
        foreach(Card card in serializedBoard.TavernCards){
            finalValue -= penaltyForHighTierInTavern * (int)CardTierList.GetCardTier(card.Name);
        }
        return finalValue;
    }

    private List<Move> NotEndTurnPossibleMoves(List<Move> possibleMoves){
        return possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
    }

    private (List<Move>, SerializedBoard) GenerateRandomTurnMoves(SerializedBoard serializedBoard, List<Move> possibleMoves){
        List<Move> notEndTurnPossibleMoves = NotEndTurnPossibleMoves(possibleMoves);
        List<Move> movesOrder = new List<Move>();
        while (notEndTurnPossibleMoves.Count != 0){
            Move chosenMove = notEndTurnPossibleMoves.PickRandom();
            movesOrder.Add(chosenMove);
            (serializedBoard, List<Move> newPossibleMoves) = serializedBoard.ApplyState(chosenMove);
            notEndTurnPossibleMoves = NotEndTurnPossibleMoves(newPossibleMoves);
        }
        return (movesOrder, serializedBoard);
    }

    private List<Move> GenerateKTurnGamesAndSelectBest(SerializedBoard serializedBoard, List<Move> possibleMoves){
        List<Move> bestPlayout = new List<Move>();
        int highestHeuristicValue = -100000000;
        int heuristicValue = 0;
        int howManySimulation = 0;
        Stopwatch s = new Stopwatch();
        s.Start();
        while (s.Elapsed < TimeSpan.FromSeconds(0.5)){
            (List<Move> generatedPlayout, SerializedBoard endTurnBoard)= GenerateRandomTurnMoves(serializedBoard, possibleMoves);
            heuristicValue = BoardStateHeuristicValueEndTurn(endTurnBoard);
            if (highestHeuristicValue < heuristicValue){
                bestPlayout = generatedPlayout;
                highestHeuristicValue = heuristicValue;
            }
            howManySimulation +=1;
        }
        /*
        int k = SimpleBots.Extensions.RandomK(1, 20);
        for (int i = 0; i < k; i++)
        {
            (List<Move> generatedPlayout, SerializedBoard endTurnBoard)= GenerateRandomTurnMoves(serializedBoard, possibleMoves);
            heuristicValue = BoardStateHeuristicValueEndTurn(endTurnBoard);
            if (highestHeuristicValue < heuristicValue){
                bestPlayout = generatedPlayout;
                highestHeuristicValue = heuristicValue;
            }
        }
        */
        log.Append(howManySimulation.ToString()+ System.Environment.NewLine);
        totalNumberOfSimulationInGame += howManySimulation;
        totalNumberOfSimulationInTurn += howManySimulation;
        return bestPlayout;
    }

    public override Move Play(SerializedBoard serializedBoard, List<Move> possibleMoves)
    {
        if (newGenarate){
            selectedTurnPlayout = GenerateKTurnGamesAndSelectBest(serializedBoard, possibleMoves);
            newGenarate = false;
        }
        if (selectedTurnPlayout.Count != 0){
            Move move = selectedTurnPlayout[0];
            selectedTurnPlayout.RemoveAt(0);
            if (!possibleMoves.Contains(move)){
                selectedTurnPlayout = GenerateKTurnGamesAndSelectBest(serializedBoard, possibleMoves);
                newGenarate = false;
                move = selectedTurnPlayout[0];
                selectedTurnPlayout.RemoveAt(0);
            }
            if (move.Command == CommandEnum.MAKE_CHOICE || move.Command == CommandEnum.BUY_CARD){
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
        log.Append("Total number of simulation in game: " + totalNumberOfSimulationInGame.ToString()+ System.Environment.NewLine);
        File.AppendAllText("log.txt", log.ToString());
        log.Clear();
    }
}
