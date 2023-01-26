using TalesOfTribute;
using TalesOfTribute.AI;
using TalesOfTribute.Board;
using TalesOfTribute.Serializers;
using TalesOfTribute.Board.Cards;
using System.Diagnostics;
using System.Text;
using TalesOfTribute.Board.CardAction;

namespace SimpleBots;

public class BeamSearchBot : AI{

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round){
        return availablePatrons.PickRandom(Rng);
    }

    private class Node{
        public Move firstMove;
        public GameState nodeGameState;
        public List<Move>? possibleMoves;
        public int heuristicScore;

        public Node(Move move, GameState gameState, List<Move>? moves, int score){
            this.firstMove = move;
            this.nodeGameState = gameState;
            this.possibleMoves = moves;
            this.heuristicScore = score;
        }

    }

    private int Heuristic(GameState gameState){
        return gameState.CurrentPlayer.Power + gameState.CurrentPlayer.Prestige;
    }

    private Move BeamSearch(GameState gameState, List<Move> possibleMoves, int k){
        List<Node> allNodes = new List<Node>();

        foreach(Move move in possibleMoves){
            if (move.Command != CommandEnum.END_TURN) {
                var (newGameState, newPossibleMoves) = gameState.ApplyState(move);
                allNodes.Add(new Node(move, newGameState, newPossibleMoves, Heuristic(newGameState)));
            } else {
                allNodes.Add(new Node(move, gameState, null, Heuristic(gameState)));
            }
        }
        
        List<Node> endStatesNodes = new List<Node>();
        List<Node> bestActualNodes = new List<Node>();

        foreach (Node node in allNodes) {
            if (node.possibleMoves is null) {
                endStatesNodes.Add(node);
                continue;
            }

            if (bestActualNodes.Count < k) {
                bestActualNodes.Add(node);
            }
            else {
                int minScore = bestActualNodes[0].heuristicScore;
                int minIndex = 0;

                for (int i = 0; i < k; i++) {
                    if (minScore > bestActualNodes[i].heuristicScore) {
                        minScore = bestActualNodes[i].heuristicScore;
                        minIndex = i;
                    }
                }

                if (minScore < node.heuristicScore) {
                    bestActualNodes[minIndex] = node;
                }
            }
        }

        while (bestActualNodes.Count > 0) {
            allNodes = new List<Node>();

            foreach (Node node in bestActualNodes) {
                foreach(Move move in node.possibleMoves){
                    if (move.Command != CommandEnum.END_TURN) {
                        var (newGameState, newPossibleMoves) = node.nodeGameState.ApplyState(move);
                        allNodes.Add(new Node(node.firstMove, newGameState, newPossibleMoves, Heuristic(newGameState)));
                    } else {
                        allNodes.Add(new Node(node.firstMove, node.nodeGameState, null, Heuristic(node.nodeGameState)));
                    }
                }
            }


            bestActualNodes = new List<Node>();

            foreach (Node node in allNodes) {
                if (node.possibleMoves is null) {
                    endStatesNodes.Add(node);
                    continue;
                }

                if (bestActualNodes.Count < k) {
                    bestActualNodes.Add(node);
                }
                else {
                    int minScore = bestActualNodes[0].heuristicScore;
                    int minIndex = 0;

                    for (int i = 0; i < k; i++) {
                        if (minScore > bestActualNodes[i].heuristicScore) {
                            minScore = bestActualNodes[i].heuristicScore;
                            minIndex = i;
                        }
                    }

                    if (minScore < node.heuristicScore) {
                        bestActualNodes[minIndex] = node;
                    }
                }
            }
        }

        Move bestMove = endStatesNodes[0].firstMove;
        int bestScore = endStatesNodes[0].heuristicScore;

        foreach (Node node in endStatesNodes) {
            if (bestScore < node.heuristicScore) {
                bestScore = node.heuristicScore;
                bestMove = node.firstMove;
            }
        }

        return bestMove;
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves){
        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN) {
            return Move.EndTurn();
        }

        return BeamSearch(gameState, possibleMoves, 20);
    }

    public override void GameEnd(EndGameState state){}
}