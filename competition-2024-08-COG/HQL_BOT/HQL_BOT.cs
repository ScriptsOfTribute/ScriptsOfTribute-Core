using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace Bots;

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
// This was added by the competition organizers to solve the problem with agents
// using this internal `List` extension.
public static class Extensions
{
    public static T PickRandom<T>(this List<T> source, SeededRandom rng)
    {
        return source[rng.Next() % source.Count];
    }
}
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

public class HQL_BOT : AI
{
    private QL ql = new QL();

    private static Random random = new Random();
    private readonly SeededRandom rng = new SeededRandom((ulong)random.Next(1000));

    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
        => availablePatrons.PickRandom(rng);

    public bool ShouldTradeCard(UniqueCard card)
    {
        return (card.Type == CardType.STARTER && card.Cost == 0) || card.Type == CardType.CURSE;
    }

    public bool ShouldUseTreasury(SeededGameState game_state, List<SimplePatronMove> patron_moves)
    {
        foreach (var move in patron_moves)
        {
            if (move.PatronId == PatronId.TREASURY)
            {
                var cards = game_state.CurrentPlayer.Hand.Concat(game_state.CurrentPlayer.Played);

                UniqueCard result = cards.First();
                foreach (var card in cards)
                {
                    if (ShouldTradeCard(card))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public UniqueCard PickCardForTreasury(SeededGameState game_state)
    {
        var cards = game_state.CurrentPlayer.Hand.Concat(game_state.CurrentPlayer.Played);

        UniqueCard result = cards.First();
        foreach (var card in cards)
        {
            if (ShouldTradeCard(card))
            {
                return card;
            }
        }

        return result;
    }

    private int nodes_limit = 10;

    public class SearchNode
    {
        public Move root_move;
        public SeededGameState node_sgs;
        public List<Move>? possible_moves;
        public int heuristic_score;

        public SearchNode(Move move, SeededGameState sgs, List<Move>? moves, int score)
        {
            root_move = move;
            node_sgs = sgs;
            possible_moves = moves;
            heuristic_score = score;
        }

    }

    private Move? FindNoChoiceMove(SeededGameState sgs, List<Move> possible_moves)
    {
        foreach (var move in possible_moves)
        {
            if (move is SimpleCardMove card)
            {
                if (NoChoiceMoves.ShouldPlay(card.Card.CommonId))
                {
                    // var (new_state, new_moves) = sgs.ApplyMove(move);
                    // if (new_state.PendingChoice == null)
                    // {
                    //     return move;
                    // }
                    return move;
                }
            }
        }

        return null;
    }

    public void PlaySimpleMovesOnNodeUntilChoice(SearchNode node)
    {
        if (node.possible_moves is null)
        {
            return;
        }

        var move = FindNoChoiceMove(node.node_sgs, node.possible_moves);
        while (move is not null)
        {
            var (new_sgs, new_possible_moves) = node.node_sgs.ApplyMove(move, (ulong)rng.Next());
            node.node_sgs = new_sgs;
            node.possible_moves = new_possible_moves;

            move = FindNoChoiceMove(node.node_sgs, node.possible_moves);
        }

        node.heuristic_score = ql.Heuristic(node.node_sgs);
    }

    private Move BestMoveFromSearch(SeededGameState sgs, List<Move> possible_moves)
    {
        List<SearchNode> all_nodes = new List<SearchNode>();

        foreach (Move move in possible_moves)
        {
            if (move.Command != CommandEnum.END_TURN)
            {
                var (newGameState, newPossibleMoves) = sgs.ApplyMove(move, (ulong)rng.Next());
                all_nodes.Add(new SearchNode(move, newGameState, newPossibleMoves, ql.Heuristic(newGameState)));
            }
            else
            {
                all_nodes.Add(new SearchNode(move, sgs, null, ql.Heuristic(sgs)));
            }
        }

        List<SearchNode> end_states_nodes = new List<SearchNode>();
        List<SearchNode> best_actual_nodes = new List<SearchNode>();

        foreach (SearchNode node in all_nodes)
        {
            if (node.possible_moves is null)
            {
                end_states_nodes.Add(node);
            }
            else
            {
                if (node.possible_moves is null)
                {
                    end_states_nodes.Add(node);
                }
                else
                {
                    best_actual_nodes.Add(node);
                }
            }
        }

        while (best_actual_nodes.Count > 0)
        {
            all_nodes = new List<SearchNode>();

            foreach (SearchNode node in best_actual_nodes)
            {
                foreach (Move move in node.possible_moves)
                {
                    if (move.Command != CommandEnum.END_TURN)
                    {
                        var (newGameState, newPossibleMoves) = node.node_sgs.ApplyMove(move, (ulong)rng.Next());
                        all_nodes.Add(new SearchNode(node.root_move, newGameState, newPossibleMoves, ql.Heuristic(newGameState)));
                    }
                    else
                    {
                        all_nodes.Add(new SearchNode(node.root_move, node.node_sgs, null, ql.Heuristic(node.node_sgs)));
                    }
                }
            }

            best_actual_nodes = new List<SearchNode>();

            foreach (SearchNode node in all_nodes)
            {
                if (node.possible_moves is null)
                {
                    end_states_nodes.Add(node);
                    continue;
                }

                if (best_actual_nodes.Count < nodes_limit)
                {
                    PlaySimpleMovesOnNodeUntilChoice(node);
                    best_actual_nodes.Add(node);
                }
                else
                {
                    int min_score = best_actual_nodes[0].heuristic_score;
                    int min_index = 0;

                    for (int i = 0; i < nodes_limit; i++)
                    {
                        if (min_score > best_actual_nodes[i].heuristic_score)
                        {
                            min_score = best_actual_nodes[i].heuristic_score;
                            min_index = i;
                        }
                    }

                    if (min_score < node.heuristic_score)
                    {
                        best_actual_nodes[min_index] = node;
                    }
                }
            }
        }

        Move best_move = end_states_nodes[0].root_move;
        int best_score = end_states_nodes[0].heuristic_score;

        foreach (SearchNode node in end_states_nodes)
        {
            if (best_score < node.heuristic_score)
            {
                best_score = node.heuristic_score;
                best_move = node.root_move;
            }
        }

        return best_move;
    }

    public void HandleEndTurn(SeededGameState sgs)
    {
        ql.IncrementTurnCounter();
        ql.SaveGainedCards(sgs);
        ql.UpdateQValuesForPlayedCardsAtEndOfTurn(sgs);
    }

    public void HandleEndPlay(SeededGameState sgs, Move best_move)
    {
        ql.SavePlayedCardIfApplicable(best_move);
        ql.UpdateDeckCardsCounter(sgs);
    }

    public override Move Play(GameState game_state, List<Move> possibleMoves, TimeSpan remainingTime)
    {
        SeededGameState sgs = game_state.ToSeededGameState((ulong)rng.Next());

        Stage stage = ql.TransformGameStateToStages(sgs);

        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN)
        {
            HandleEndPlay(sgs, possibleMoves[0]);
            HandleEndTurn(sgs);
            return Move.EndTurn();
        }

        var action_agent_moves = possibleMoves.Where(m => m.Command == CommandEnum.PLAY_CARD ||
            m.Command == CommandEnum.ACTIVATE_AGENT).ToList();

        var buy_moves = possibleMoves.Where(m => m.Command == CommandEnum.BUY_CARD).ToList();
        var patron_moves = possibleMoves.Where(m => m.Command == CommandEnum.CALL_PATRON).ToList().ConvertAll(m => (SimplePatronMove)m);

        Move best_move = possibleMoves[0];

        if (action_agent_moves.Count != 0)
        {
            best_move = action_agent_moves[0];
            var no_choice_move = FindNoChoiceMove(sgs, action_agent_moves);
            if (no_choice_move is not null)
            {
                best_move = no_choice_move;
            }
        }
        // else if (buy_moves.Count != 0)
        // {
        //     // best_move = ql.PickBuyMove(sgs, buy_moves);
        // }
        else
        {
            for (int i = 0; i < buy_moves.Count; i++)
            {
                if (buy_moves[i] is SimpleCardMove buy_card)
                {
                    if (buy_card.Card.Type is CardType.CONTRACT_ACTION)
                    {
                        var (new_state, new_moves) = sgs.ApplyMove(buy_moves[i]);

                        var old_score = ql.Heuristic(sgs);
                        var new_score = ql.Heuristic(new_state);

                        if (old_score > new_score)
                        {
                            possibleMoves.RemoveAll(m => m == buy_moves[i]);
                        }
                    }
                }
            }

            for (int i = possibleMoves.Count - 1; i >= 0; i--)
            {
                if (possibleMoves[i] is SimplePatronMove patron)
                {
                    if (patron.PatronId == PatronId.DUKE_OF_CROWS && stage != Stage.Late && sgs.CurrentPlayer.Coins < 7)
                    {
                        possibleMoves.RemoveAt(i);
                        break;
                    }
                }
            }
            // for (int i = possibleMoves.Count - 1; i >= 0; i--)
            // {
            //     if (possibleMoves[i] is SimplePatronMove patron)
            //     {
            //         if (patron.PatronId == PatronId.ORGNUM && (stage != Stage.Late || stage != Stage.Middle))
            //         {
            //             possibleMoves.RemoveAt(i);
            //             break;
            //         }
            //     }
            // }
            best_move = BestMoveFromSearch(sgs, possibleMoves);
        }

        HandleEndPlay(sgs, best_move);
        if (best_move.Command == CommandEnum.END_TURN)
        {
            HandleEndTurn(sgs);
        }

        return best_move;
    }

    public override void GameEnd(EndGameState state, FullGameState? final_board_state)
    {
        ql.ResetVariables();

        ql.SaveQTableToFile();
    }
}
