using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.Board.Cards;
using System.Diagnostics;
using System.Text;
using ScriptsOfTribute.Board.CardAction;

namespace SimpleBots;

public class BeamSearchBot : AI
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
    private int k = 20;
    private Random myRNG = new Random();
    private string patronLogPath = "patronBeamSearchBot.txt";
    private Apriori apriori = new Apriori();
    private int support = 4;
    private double confidence = 0.3;
    private PlayerEnum myID;
    private string patrons;
    private bool startOfGame = true;
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        //PatronId? selectedPatron = apriori.AprioriBestChoice(availablePatrons, patronLogPath, support, confidence);
        //return selectedPatron ?? availablePatrons.PickRandom(Rng);
        return availablePatrons.PickRandom(Rng);
    }

    private class Node
    {
        public Move firstMove;
        public GameState nodeGameState;
        public List<Move>? possibleMoves;
        public int heuristicScore;

        public Node(Move move, GameState gameState, List<Move>? moves, int score)
        {
            this.firstMove = move;
            this.nodeGameState = gameState;
            this.possibleMoves = moves;
            this.heuristicScore = score;
        }

    }

    private int Heuristic(GameState gameState)
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

        finalValue += gameState.CurrentPlayer.Power * powerValue;
        finalValue += gameState.CurrentPlayer.Prestige * prestigeValue;


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
            List<UniqueCard> allCardsEnemy = gameState.EnemyPlayer.HandAndDraw.Concat(gameState.CurrentPlayer.Played.Concat(gameState.CurrentPlayer.CooldownPile)).ToList();
            Dictionary<PatronId, int> potentialComboNumberEnemy = new Dictionary<PatronId, int>();

            foreach (UniqueCard card in allCards)
            {
                tier = CardTierList.GetCardTier(card.Name);
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

            foreach (UniqueCard card in allCardsEnemy)
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
        }
        return finalValue;
        //return gameState.CurrentPlayer.Power + gameState.CurrentPlayer.Prestige;
    }

    private Move BeamSearch(GameState gameState, List<Move> possibleMoves, int k)
    {
        List<Node> allNodes = new List<Node>();

        foreach (Move move in possibleMoves)
        {
            if (move.Command != CommandEnum.END_TURN)
            {
                var (newGameState, newPossibleMoves) = gameState.ApplyState(move);
                allNodes.Add(new Node(move, newGameState, newPossibleMoves, Heuristic(newGameState)));
            }
            else
            {
                allNodes.Add(new Node(move, gameState, null, Heuristic(gameState)));
            }
        }

        List<Node> endStatesNodes = new List<Node>();
        List<Node> bestActualNodes = new List<Node>();

        foreach (Node node in allNodes)
        {
            if (node.possibleMoves is null)
            {
                endStatesNodes.Add(node);
                continue;
            }

            if (bestActualNodes.Count < k)
            {
                bestActualNodes.Add(node);
            }
            else
            {
                int minScore = bestActualNodes[0].heuristicScore;
                int minIndex = 0;

                for (int i = 0; i < k; i++)
                {
                    if (minScore > bestActualNodes[i].heuristicScore)
                    {
                        minScore = bestActualNodes[i].heuristicScore;
                        minIndex = i;
                    }
                }

                if (minScore < node.heuristicScore)
                {
                    bestActualNodes[minIndex] = node;
                }
            }
        }

        int temp = 4;
        int iteration = 0;
        while (bestActualNodes.Count > 0)
        {
            allNodes = new List<Node>();

            foreach (Node node in bestActualNodes)
            {
                foreach (Move move in node.possibleMoves)
                {
                    if (move.Command != CommandEnum.END_TURN)
                    {
                        var (newGameState, newPossibleMoves) = node.nodeGameState.ApplyState(move);
                        allNodes.Add(new Node(node.firstMove, newGameState, newPossibleMoves, Heuristic(newGameState)));
                    }
                    else
                    {
                        allNodes.Add(new Node(node.firstMove, node.nodeGameState, null, Heuristic(node.nodeGameState)));
                    }
                }
            }


            bestActualNodes = new List<Node>();

            foreach (Node node in allNodes)
            {
                if (node.possibleMoves is null)
                {
                    endStatesNodes.Add(node);
                    continue;
                }

                if (bestActualNodes.Count < k)
                {
                    bestActualNodes.Add(node);
                }
                else
                {
                    int minScore = bestActualNodes[0].heuristicScore;
                    int minIndex = 0;

                    for (int i = 0; i < k; i++)
                    {
                        if (minScore > bestActualNodes[i].heuristicScore)
                        {
                            minScore = bestActualNodes[i].heuristicScore;
                            minIndex = i;
                        }
                    }

                    if (minScore < node.heuristicScore)
                    {
                        bestActualNodes[minIndex] = node;
                    }
                    else
                    {
                        if (Math.Exp((-(node.heuristicScore - minScore))) / (temp / (iteration + 1)) > myRNG.NextDouble())
                        {
                            bestActualNodes[minIndex] = node;
                        }
                    }
                }
            }
            iteration++;
        }

        Move bestMove = endStatesNodes[0].firstMove;
        int bestScore = endStatesNodes[0].heuristicScore;

        foreach (Node node in endStatesNodes)
        {
            if (bestScore < node.heuristicScore)
            {
                bestScore = node.heuristicScore;
                bestMove = node.firstMove;
            }
        }

        return bestMove;
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        if (startOfGame)
        {
            myID = gameState.CurrentPlayer.PlayerID;
            patrons = string.Join(",", gameState.Patrons.FindAll(x => x != PatronId.TREASURY).Select(n => n.ToString()).ToArray());
            startOfGame = false;
        }

        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN)
        {
            return Move.EndTurn();
        }

        return BeamSearch(gameState, possibleMoves, k);
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        if (state.Winner == myID)
        {
            File.AppendAllText(patronLogPath, patrons + System.Environment.NewLine);
        }
        startOfGame = true;
    }
}