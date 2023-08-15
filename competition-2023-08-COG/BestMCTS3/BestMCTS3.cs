using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

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

public class MCTSNode
{
    public List<(MCTSNode?, Move)> children = new();
    public SeededGameState gameState;
    public double score = 0;
    public int visits = 0;
    public bool full = false;
    public bool endTurn;
    public MCTSNode(SeededGameState myState, List<Move>? possibleMoves = null)
    {
        gameState = myState;
        if (possibleMoves is not null)
        {
            children = possibleMoves.ConvertAll<(MCTSNode?, Move)>(move => (null, move));
            endTurn = false;
        }
        else
        {
            full = true;
            endTurn = true;
        }
    }

    public static double UCBScore(MCTSNode? child, int parentSimulations)
    {
        if (child is null || child.visits == 0)
        {
            return double.MaxValue;
        }

        if (child.full)
        {
            return double.MinValue;
        }

        return child.score + 1.41 * Math.Sqrt(Math.Log(parentSimulations) / child.visits);
    }

    public double Simulate(MCTSNode node, GameStrategy strategy, SeededRandom rng, ref int moveCount)
    {
        if (node.endTurn || (node.children.Count == 1 && node.children[0].Item2.Command == CommandEnum.END_TURN))
        {
            return strategy.Heuristic(node.gameState);
        }

        SeededGameState gameState = node.gameState;
        List<Move> possibleMoves = node.children.ConvertAll<Move>(m => m.Item2);
        List<Move> notEndMoves = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
        Move move = notEndMoves.PickRandom(rng);

        while (move.Command != CommandEnum.END_TURN)
        {
            moveCount += 1;
            (gameState, possibleMoves) = gameState.ApplyMove(move);
            notEndMoves = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();

            if (notEndMoves.Count > 0)
            {
                move = notEndMoves.PickRandom(rng);
            }
            else
            {
                move = Move.EndTurn();
            }

        }

        return strategy.Heuristic(gameState);
    }
}


public class BestMCTS3 : AI
{
    static readonly Random rnd = new();
    readonly List<ulong> seeds = new();
    readonly List<SeededRandom> rngs = new();
    const int maxNoOfRoots = 3;
    int noOfRoots = maxNoOfRoots;
    readonly MCTSNode?[] roots = new MCTSNode?[maxNoOfRoots];
    TimeSpan usedTimeInTurn = TimeSpan.FromSeconds(0);
    readonly TimeSpan turnTimeout = TimeSpan.FromSeconds(9.9);
    bool startOfTurn = true;
    GameStrategy strategy = new(10, GamePhase.EarlyGame);

    public BestMCTS3()
    {
        for (int i = 0; i < maxNoOfRoots; i++)
        {
            seeds.Add((ulong)rnd.Next());
            rngs.Add(new(seeds[i]));
        }
    }


    List<Move> FilterMoves(List<Move> moves, SeededGameState gameState)
    {
        moves.Sort(new MoveComparer());
        if (moves.Count == 1) return moves;
        if (gameState.BoardState == BoardState.CHOICE_PENDING)
        {
            List<Move> toReturn = new();
            switch (gameState.PendingChoice!.ChoiceFollowUp)
            {
                case ChoiceFollowUp.COMPLETE_TREASURY:
                    List<Move> gold = new();
                    foreach (Move mv in moves)
                    {
                        var mcm = mv as MakeChoiceMove<UniqueCard>;
                        UniqueCard card = mcm!.Choices[0];
                        if (card.CommonId == CardId.BEWILDERMENT) return new List<Move> { mv };
                        if (card.CommonId == CardId.GOLD && gold.Count == 0) gold.Add(mv);
                        if (card.Cost == 0) toReturn.Add(mv); // moze tez byc card.Type == 'Starter'
                    }
                    if (gold.Count == 1) return gold;
                    if (toReturn.Count > 0) return toReturn;
                    return new List<Move> { moves[0] };
                case ChoiceFollowUp.DESTROY_CARDS:
                    List<(Move, double)> choices = new();
                    foreach (Move mv in moves)
                    {
                        var mcm = mv as MakeChoiceMove<UniqueCard>;
                        if (mcm!.Choices.Count != 1) continue;
                        choices.Add((mv, strategy.CardEvaluation(mcm!.Choices[0], gameState)));
                    }
                    choices.Sort(new PairOnlySecond());
                    List<CardId> cards = new();
                    for (int i = 0; i < Math.Min(3, choices.Count); i++)
                    {
                        var mcm = choices[i].Item1 as MakeChoiceMove<UniqueCard>;
                        cards.Add(mcm!.Choices[0].CommonId);
                    }
                    foreach (Move mv in moves)
                    {
                        var mcm = mv as MakeChoiceMove<UniqueCard>;
                        bool flag = true;
                        foreach (UniqueCard card in mcm!.Choices)
                        {
                            if (!cards.Contains(card.CommonId))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag) toReturn.Add(mv);
                    }
                    if (toReturn.Count > 0) return toReturn;
                    return moves;
                case ChoiceFollowUp.REFRESH_CARDS: // tu i tak musi byc duzo wierzcholkow i guess
                    List<(Move, double)> possibilities = new();
                    foreach (Move mv in moves)
                    {
                        var mcm = mv as MakeChoiceMove<UniqueCard>;
                        double val = 0;
                        foreach (UniqueCard card in mcm!.Choices)
                        {
                            val += strategy.CardEvaluation(card, gameState);
                        }
                        possibilities.Add((mv, val));
                    }
                    possibilities.Sort(new PairOnlySecond());
                    possibilities.Reverse();
                    if (gameState.PendingChoice.MaxChoices == 3)
                    {
                        for (int i = 0; i < Math.Min(10, possibilities.Count); i++)
                        {
                            toReturn.Add(possibilities[i].Item1);
                        }
                    }
                    if (gameState.PendingChoice.MaxChoices == 2)
                    {
                        for (int i = 0; i < Math.Min(6, possibilities.Count); i++)
                        {
                            toReturn.Add(possibilities[i].Item1);
                        }
                    }
                    if (gameState.PendingChoice.MaxChoices == 1)
                    {
                        for (int i = 0; i < Math.Min(3, possibilities.Count); i++)
                        {
                            toReturn.Add(possibilities[i].Item1);
                        }
                    }
                    if (toReturn.Count == 0) return moves;
                    return toReturn;
                default:
                    return moves;
            }
        }
        foreach (Move mv in moves)
        {
            if (mv.Command == CommandEnum.PLAY_CARD)
            {
                var mvCopy = mv as SimpleCardMove;
                if (InstantPlayCards.IsInstantPlay(mvCopy!.Card.CommonId))
                {
                    return new List<Move> { mv };
                }
            }
        }
        return moves;
    }

    double Run(MCTSNode node, SeededRandom rng, ref int moveCount)
    {
        if (node.endTurn || node.visits == 0)
        {
            double score = node.Simulate(node, strategy, rng, ref moveCount);
            node.visits += 1;
            node.score = score;
            return score;
        }

        moveCount += 1;

        double maxScore = double.MinValue;
        int selectedChild = 0;

        int index = 0;
        foreach (var (child, move) in node.children)
        {
            double score = MCTSNode.UCBScore(child, node.visits);
            if (score > maxScore)
            {
                maxScore = score;
                selectedChild = index;
            }
            index += 1;
        }

        if (node.children[selectedChild].Item1 is null)
        {
            var move = node.children[selectedChild].Item2;
            if (move.Command == CommandEnum.END_TURN)
            {
                node.children[selectedChild] = (new MCTSNode(node.gameState), move);
            }
            else
            {
                var (childGameState, childPossibleMoves) = node.gameState.ApplyMove(move);
                List<Move> filteredMoves = FilterMoves(childPossibleMoves, childGameState);
                node.children[selectedChild] = (new MCTSNode(childGameState, filteredMoves), move);
            }
        }

        double result = Run(node.children[selectedChild].Item1!, rng, ref moveCount);

        bool isNodeFull = true;
        foreach (var (child, move) in node.children)
        {
            isNodeFull &= child is not null && child.full;
        }

        node.full = isNodeFull;
        node.visits += 1;
        node.score = Math.Max(node.score, result);
        return result;
    }

    static public bool CheckIfSameCards(List<UniqueCard> l, List<UniqueCard> r)
    {
        var balance = new Dictionary<CardId, int>();

        foreach (UniqueCard card in l)
        {
            if (balance.ContainsKey(card.CommonId))
            {
                balance[card.CommonId] += 1;
            }
            else
            {
                balance[card.CommonId] = 1;
            }
        }
        foreach (UniqueCard card in r)
        {
            if (balance.ContainsKey(card.CommonId))
            {
                balance[card.CommonId] -= 1;
            }
            else
            {
                return false;
            }
        }

        return balance.Values.All(cnt => cnt == 0);
    }
    bool CheckIfSameGameStateAfterOneMove(MCTSNode node, GameState gameState)
    {
        return CheckIfSameCards(node.gameState.CurrentPlayer.Hand, gameState.CurrentPlayer.Hand)
             && CheckIfSameCards(node.gameState.TavernAvailableCards, gameState.TavernAvailableCards)
             && CheckIfSameCards(node.gameState.CurrentPlayer.CooldownPile, gameState.CurrentPlayer.CooldownPile) // chyba niepotrzebne
             && CheckIfSameCards(node.gameState.CurrentPlayer.DrawPile, gameState.CurrentPlayer.DrawPile)    // chyba niepotrzebne
        ;
    }

    static public bool CheckIfSameEffects(List<UniqueEffect> e1, List<UniqueEffect> e2)
    {
        var balance = new Dictionary<(CardId, EffectType, int, int), int>();
        foreach (UniqueEffect ef in e1)
        {
            if (balance.ContainsKey((ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)))
            {
                balance[(ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)] += 1;
            }
            else
            {
                balance[(ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)] = 1;
            }
        }
        foreach (UniqueEffect ef in e2)
        {
            if (balance.ContainsKey((ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)))
            {
                balance[(ef.ParentCard.CommonId, ef.Type, ef.Amount, ef.Combo)] -= 1;
            }
            else
            {
                return false;
            }
        }

        return balance.Values.All(cnt => cnt == 0);
    }
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        if (availablePatrons.Contains(PatronId.DUKE_OF_CROWS)) return PatronId.DUKE_OF_CROWS;
        if (availablePatrons.Contains(PatronId.RED_EAGLE)) return PatronId.RED_EAGLE;
        return availablePatrons.PickRandom(rngs[0]);
    }

    void SelectStrategy(GameState gameState)
    {
        var currentPlayer = gameState.CurrentPlayer;
        int cardCount = currentPlayer.Hand.Count + currentPlayer.CooldownPile.Count + currentPlayer.DrawPile.Count;
        int points = gameState.CurrentPlayer.Prestige;
        if (points >= 27 || gameState.EnemyPlayer.Prestige >= 30)
        {
            strategy = new GameStrategy(cardCount, GamePhase.LateGame);
        }
        else if (points <= 10 && gameState.EnemyPlayer.Prestige <= 13)
        {
            strategy = new GameStrategy(cardCount, GamePhase.EarlyGame);
        }
        else
        {
            strategy = new GameStrategy(cardCount, GamePhase.MidGame);
        }
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves)
    {
        Stopwatch moveTime = new();
        moveTime.Start();

        if (startOfTurn)
        {
            noOfRoots = maxNoOfRoots;
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            SelectStrategy(gameState);
        }

        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN)
        {
            startOfTurn = true;
            return Move.EndTurn();
        }

        if (startOfTurn)
        {
            for (int i = 0; i < noOfRoots; i++)
            {
                SeededGameState seededGameState = gameState.ToSeededGameState(seeds[i]);
                List<Move> filteredMoves = FilterMoves(possibleMoves, seededGameState);
                roots[i] = new MCTSNode(seededGameState, filteredMoves);
            }
            startOfTurn = false;
        }

        for (int i = 0; i < noOfRoots; i++)
        {
            if (!CheckIfSameGameStateAfterOneMove(roots[i]!, gameState))
            {
                SeededGameState seededGameState = gameState.ToSeededGameState(seeds[i]);
                List<Move> filteredMoves = FilterMoves(possibleMoves, seededGameState);
                roots[i] = new MCTSNode(seededGameState, filteredMoves);
            }
        }


        Move move;
        Move moveOut = possibleMoves.PickRandom(rngs[0]);

        TimeSpan timeForMoveComputation;
        if (turnTimeout - moveTime.Elapsed - usedTimeInTurn > TimeSpan.FromSeconds(5.0))
        {
            int maxNoOfMoves = 0;
            for (int i = 0; i < 20; i++)
            {
                int ile = 0;
                Run(roots[i % noOfRoots]!, rngs[i % noOfRoots], ref ile);
                maxNoOfMoves = Math.Max(ile, maxNoOfMoves);
            }
            timeForMoveComputation = (turnTimeout - moveTime.Elapsed - usedTimeInTurn) / Math.Max(2, maxNoOfMoves + 1);
            if (timeForMoveComputation > TimeSpan.FromSeconds(0.5))
            {
                timeForMoveComputation = TimeSpan.FromSeconds(0.5);
            }
        }
        else
        {
            noOfRoots = 1;
            int maxNoOfMoves = 0;
            for (int i = 0; i < 20; i++)
            {
                int ile = 0;
                Run(roots[0]!, rngs[0], ref ile);
                maxNoOfMoves = Math.Max(ile, maxNoOfMoves);
            }
            timeForMoveComputation = (turnTimeout - moveTime.Elapsed - usedTimeInTurn) / Math.Max(2, maxNoOfMoves + 1);
            if (timeForMoveComputation > TimeSpan.FromSeconds(0.8))
            {
                timeForMoveComputation = TimeSpan.FromSeconds(0.8);
            }
        }

        if (timeForMoveComputation < TimeSpan.FromSeconds(0.1))
        {
            if (moveOut.Command == CommandEnum.END_TURN)
            {
                startOfTurn = true;
            }
            usedTimeInTurn += moveTime.Elapsed;
            return moveOut;
        }


        int spam = 0;
        for (int i = 0; i < noOfRoots; i++)
        {
            Stopwatch s = new();
            s.Start();
            while (s.Elapsed < timeForMoveComputation / noOfRoots && !roots[i]!.full)
            {
                Run(roots[i]!, rngs[i], ref spam);
            }
        }


        int idx = 0;
        double val = 0;
        for (int j = 0; j < roots[0]!.children.Count; j++)
        {
            double maks = 0;
            double min = 1;
            double cur = 0;
            for (int i = 0; i < noOfRoots; i++)
            {
                double score = roots[i]!.children[j].Item1!.score;
                cur += score;
                min = Math.Min(min, score);
                maks = Math.Max(maks, score);
            }
            if (noOfRoots >= 4)
            {
                cur -= maks + min;
            }
            if (cur > val)
            {
                val = cur;
                idx = j;
            }
        }

        move = roots[0]!.children[idx].Item2;
        for (int i = 0; i < noOfRoots; i++)
        {
            roots[i] = roots[i]!.children[idx].Item1;
        }

        foreach (Move mv in possibleMoves)
        {
            if (MoveComparer.AreIsomorphic(move, mv))
            {
                moveOut = mv;
                break;
            }
        }

        if (moveOut.Command == CommandEnum.END_TURN)
        {
            usedTimeInTurn = TimeSpan.FromSeconds(0);
            startOfTurn = true;
        }
        usedTimeInTurn += moveTime.Elapsed;

        return moveOut;
    }

    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
    }
}


public class PairOnlySecond : Comparer<(Move, double)>
{
    public override int Compare((Move, double) a, (Move, double) b)
    {
        return a.Item2.CompareTo(b.Item2);
    }
}
public class MoveComparer : Comparer<Move>
{
    public static ulong HashMove(Move x)
    {
        ulong hash = 0;

        if (x.Command == CommandEnum.CALL_PATRON)
        {
            var mx = x as SimplePatronMove;
            hash = (ulong)mx!.PatronId;
        }
        else if (x.Command == CommandEnum.MAKE_CHOICE)
        {
            var mx = x as MakeChoiceMove<UniqueCard>;
            if (mx is not null)
            {
                var ids = mx!.Choices.Select(card => (ulong)card.CommonId).OrderBy(id => id);
                foreach (ulong id in ids) hash = hash * 200UL + id;
            }
            else
            {
                var mxp = x as MakeChoiceMove<UniqueEffect>;
                var ids = mxp!.Choices.Select(ef => (ulong)ef.Type).OrderBy(type => type);
                foreach (ulong id in ids) hash = hash * 200UL + id;
                hash += 1_000_000_000UL;
            }
        }
        else if (x.Command != CommandEnum.END_TURN)
        {
            var mx = x as SimpleCardMove;
            hash = (ulong)mx!.Card.CommonId;
        }
        return hash + 1_000_000_000_000UL * (ulong)x.Command;
    }
    public override int Compare(Move x, Move y)
    {
        ulong hx = HashMove(x);
        ulong hy = HashMove(y);
        return hx.CompareTo(hy);
    }

    public static bool AreIsomorphic(Move move1, Move move2)
    {
        if (move1.Command != move2.Command) return false; // Speed up
        return HashMove(move1) == HashMove(move2);
    }
}
