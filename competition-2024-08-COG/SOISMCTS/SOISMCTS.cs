using ScriptsOfTribute;
using ScriptsOfTribute.AI;
using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;
using System.Diagnostics;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection.Emit;

//note this bot has dependencies on AgentTierList.cs, HandTierList.cs, GameStrategy.cs, TreeReporter.cs,
//and MoveComparer in BestMCTS3.cs
//AgentTierList and HandTierList have been updated since use in last competition to include new cards.

/*

Note from the organizers: Since BestMCTS3 is also participating in this year's contest, 
the class names in this bot have been modified by adding "SOISMCTS" to the names.
The exception is two enums located in GameStrategy.cs and GameStrategySOISMCTS.cs - 
they are identical for both implementations and used in many places.
Therefore, this part of the code has been commented out for the contest to prevent errors.
*/
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

public enum TierEnum
{
    /*
    S = 1000,
    A = 400,
    B = 200,
    C = 90,
    D = 40,
    */
    S = 50,
    A = 25,
    B = 10,
    C = 5,
    D = 1,
    UNKNOWN = 0,
}

public class CardTier
{
    public string Name;
    public PatronId Deck;
    public TierEnum Tier;

    public CardTier(string name, PatronId deck, TierEnum tier)
    {
        Name = name;
        Deck = deck;
        Tier = tier;
    }
}

public class CardTierList
{
    private static CardTier[] CardTierArray = {
        new CardTier("Currency Exchange", PatronId.HLAALU, TierEnum.S),
        new CardTier("Luxury Exports", PatronId.HLAALU, TierEnum.S),
        new CardTier("Oathman", PatronId.HLAALU, TierEnum.A),
        new CardTier("Ebony Mine", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hlaalu Councilor", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hlaalu Kinsman", PatronId.HLAALU, TierEnum.B),
        new CardTier("House Embassy", PatronId.HLAALU, TierEnum.B),
        new CardTier("House Marketplace", PatronId.HLAALU, TierEnum.B),
        new CardTier("Hireling", PatronId.HLAALU, TierEnum.C),
        new CardTier("Hostile Takeover", PatronId.HLAALU, TierEnum.C),
        new CardTier("Kwama Egg Mine", PatronId.HLAALU, TierEnum.C),
        new CardTier("Customs Seizure", PatronId.HLAALU, TierEnum.D),
        new CardTier("Goods Shipment", PatronId.HLAALU, TierEnum.D),
        new CardTier("Midnight Raid", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Blood Sacrifice", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Bloody Offering", PatronId.RED_EAGLE, TierEnum.S),
        new CardTier("Bonfire", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Briarheart Ritual", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Clan-Witch", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("Elder Witch", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Hagraven", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Hagraven Matron", PatronId.RED_EAGLE, TierEnum.A),
        new CardTier("Imperial Plunder", PatronId.RED_EAGLE, TierEnum.A),
        new CardTier("Imperial Spoils", PatronId.RED_EAGLE, TierEnum.B),
        new CardTier("Karth Man-Hunter", PatronId.RED_EAGLE, TierEnum.C),
        new CardTier("War Song", PatronId.RED_EAGLE, TierEnum.D),
        new CardTier("Blackfeather Knave", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Plunder", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Toll of Flesh", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Toll of Silver", PatronId.DUKE_OF_CROWS, TierEnum.S),
        new CardTier("Murder of Crows", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Pilfer", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Squawking Oratory", PatronId.DUKE_OF_CROWS, TierEnum.A),
        new CardTier("Law of Sovereign Roost", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Pool of Shadow", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Scratch", PatronId.DUKE_OF_CROWS, TierEnum.B),
        new CardTier("Blackfeather Brigand", PatronId.DUKE_OF_CROWS, TierEnum.C),
        new CardTier("Blackfeather Knight", PatronId.DUKE_OF_CROWS, TierEnum.C),
        new CardTier("Peck", PatronId.DUKE_OF_CROWS, TierEnum.D),
        new CardTier("Conquest", PatronId.ANSEI, TierEnum.S),
        new CardTier("Grand Oratory", PatronId.ANSEI, TierEnum.S),
        new CardTier("Hira's End", PatronId.ANSEI, TierEnum.S),
        new CardTier("Hel Shira Herald", PatronId.ANSEI, TierEnum.A),
        new CardTier("March on Hattu", PatronId.ANSEI, TierEnum.A),
        new CardTier("Shehai Summoning", PatronId.ANSEI, TierEnum.A),
        new CardTier("Warrior Wave", PatronId.ANSEI, TierEnum.A),
        new CardTier("Ansei Assault", PatronId.ANSEI, TierEnum.B),
        new CardTier("Ansei's Victory", PatronId.ANSEI, TierEnum.B),
        new CardTier("Battle Meditation", PatronId.ANSEI, TierEnum.B),
        new CardTier("No Shira Poet", PatronId.ANSEI, TierEnum.C),
        new CardTier("Way of the Sword", PatronId.ANSEI, TierEnum.D),
        new CardTier("Prophesy", PatronId.PSIJIC, TierEnum.S),
        new CardTier("Scrying Globe", PatronId.PSIJIC, TierEnum.S),
        new CardTier("The Dreaming Cave", PatronId.PSIJIC, TierEnum.S),
        new CardTier("Augur's Counsel", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Psijic Relicmaster", PatronId.PSIJIC, TierEnum.A),
        new CardTier("Sage Counsel", PatronId.PSIJIC, TierEnum.A),
        new CardTier("Prescience", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Psijic Apprentice", PatronId.PSIJIC, TierEnum.B),
        new CardTier("Ceporah's Insight", PatronId.PSIJIC, TierEnum.C),
        new CardTier("Psijic's Insight", PatronId.PSIJIC, TierEnum.C),
        new CardTier("Time Mastery", PatronId.PSIJIC, TierEnum.D),
        new CardTier("Mainland Inquiries", PatronId.PSIJIC, TierEnum.D),
        new CardTier("Rally", PatronId.PELIN, TierEnum.S),
        new CardTier("Siege Weapon Volley", PatronId.PELIN, TierEnum.S),
        new CardTier("The Armory", PatronId.PELIN, TierEnum.S),
        new CardTier("Banneret", PatronId.PELIN, TierEnum.A),
        new CardTier("Knight Commander", PatronId.PELIN, TierEnum.A),
        new CardTier("Reinforcements", PatronId.PELIN, TierEnum.A),
        new CardTier("Archers' Volley", PatronId.PELIN, TierEnum.B),
        new CardTier("Legion's Arrival", PatronId.PELIN, TierEnum.B),
        new CardTier("Shield Bearer", PatronId.PELIN, TierEnum.B),
        new CardTier("Bangkorai Sentries", PatronId.PELIN, TierEnum.C),
        new CardTier("Knights of Saint Pelin", PatronId.PELIN, TierEnum.C),
        new CardTier("The Portcullis", PatronId.PELIN, TierEnum.D),
        new CardTier("Fortify", PatronId.PELIN, TierEnum.D),
        new CardTier("Bag of Tricks", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Bewilderment", PatronId.RAJHIN, TierEnum.D),
        new CardTier("Grand Larceny", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Jarring Lullaby", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Jeering Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Moonlit Illusion", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Pounce and Profit", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Prowling Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Ring's Guile", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Shadow's Slumber", PatronId.RAJHIN, TierEnum.A),
        new CardTier("Slight of Hand", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Stubborn Shadow", PatronId.RAJHIN, TierEnum.B),
        new CardTier("Swipe", PatronId.RAJHIN, TierEnum.D),
        new CardTier("Twilight Revelry", PatronId.RAJHIN, TierEnum.S),
        new CardTier("Ghostscale Sea Serpent", PatronId.ORGNUM, TierEnum.B),
        new CardTier("King Orgnum's Command", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Maormer Boarding Party", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Maormer Cutter", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Pyandonean War Fleet", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Sea Elf Raid", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Sea Raider's Glory", PatronId.ORGNUM, TierEnum.C),
        new CardTier("Sea Serpent Colossus", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Serpentguard Rider", PatronId.ORGNUM, TierEnum.A),
        new CardTier("Serpentprow Schooner", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Snakeskin Freebooter", PatronId.ORGNUM, TierEnum.S),
        new CardTier("Storm Shark Wavecaller", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Summerset Sacking", PatronId.ORGNUM, TierEnum.B),
        new CardTier("Ambush", PatronId.TREASURY, TierEnum.B),
        new CardTier("Barterer", PatronId.TREASURY, TierEnum.C),
        new CardTier("Black Sacrament", PatronId.TREASURY, TierEnum.B),
        new CardTier("Blackmail", PatronId.TREASURY, TierEnum.B),
        new CardTier("Gold", PatronId.TREASURY, TierEnum.UNKNOWN),
        new CardTier("Harvest Season", PatronId.TREASURY, TierEnum.C),
        new CardTier("Imprisonment", PatronId.TREASURY, TierEnum.C),
        new CardTier("Ragpicker", PatronId.TREASURY, TierEnum.C),
        new CardTier("Tithe", PatronId.TREASURY, TierEnum.C),
        new CardTier("Writ of Coin", PatronId.TREASURY, TierEnum.D),
        new CardTier("Unknown", PatronId.TREASURY, TierEnum.UNKNOWN)
    };

    public static TierEnum GetCardTier(string cardName)
    {
        return Array.Find(CardTierArray, x => x.Name == cardName).Tier;
    }
}
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

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

//class to implement single observer information set monte carlo tree search. This implementation only 
//searches a tree of moves for the current player's turn, so the observer is the current player.
public class SOISMCTS : AI
{
    //run configuration paraameters
    
    //parameters for re-using tree between moves
    private bool _treeReuse = false;
    private InfosetNode? _reusedRootNode;
    
    //parameters for random determinisations
    private bool _randomDeterminisations = false;
    private int _noSimsPerRandomisation = 150;
    
    //Parameters and data structure for MAST statistics
    private bool _useMAST = false;
    private double _temperature = 1.0; //parameter for gibbs distribution
    private double _thresholdForRandomMoves = 0.1; //parameter to choose a random move instead of move from Gibbs distribution
    private Dictionary<MASTMoveKey, (double totalReward, int count)> mastStats;
    
    //boolean to turn move filtering on and off
    private bool _filterMoves = true;
    
    //boolean to choose heuristic, if false use BestMCTS3 bot heuristic
    private bool _useMCTSBotHeuristic = false;
    
    //parameter to choose between max reward and visit count for move selection. If false uses visit count
    private bool _useMaxReward = true;
    
    //parameters for MCTS
    private readonly double K = Math.Sqrt(2); 
    
    //parameters for computational budget
    //turn on time allocation by move
    private bool _timeAllocation = false;
    //we use the expected number of  moves by turn to allocation computational budget
    //this is calibrated by running 100 games against MCTSBot3 and calculating average number of moves per turn
    // private Dictionary<int, double> _expNoMovesPerTurn = new Dictionary<int, double>
    // {
    //     {0, 8.02}, {1, 7.91}, {2, 8.82}, {3, 8.2}, {4, 9}, {5, 9.56}, {6, 9.7}, {7, 9.34}, {8, 9.78}, 
    //     {9, 9.79}, {10, 8.73}, {11, 8.31}, {12, 6.17}, {13, 5.5}, {14, 4}, {15, 2.91}, {16, 2.17}, {17, 1.48}, 
    //     {18, 1.06}, {19, 0.66}, {20, 0.36}, {21, 0.32}, {22, 0.2 }, {23, 0.17}, {24, 0.08}
    // };
    // //round up expected number of moves per turn to help avoid timeouts
    private Dictionary<int, double> _expNoMovesPerTurn = new Dictionary<int, double>
    {
        {0, 8}, {1, 8}, {2, 9}, {3, 9}, {4, 9}, {5, 10}, {6, 10}, {7, 10}, {8, 10}, 
        {9, 10}, {10, 9}, {11, 9}, {12, 7}, {13, 6}, {14, 4}, {15, 3}, {16, 3}, {17, 2}, 
        {18, 1}, {19, 1}, {20, 1}, {21, 1}, {22, 1}, {23, 1}, {24, 1}
    };
    private TimeSpan _usedTimeInTurn = TimeSpan.FromSeconds(0); //tracks how long we are spending on current turn
    private TimeSpan _timeForMoveComputation; //time limit for current move
    private readonly TimeSpan _turnTimeout = TimeSpan.FromSeconds(9.9); //total time limit for turn based on competition rules
    //the can be used ot calibrate the number of expected moves per turn for the dictionary above
    //private Dictionary<int, int> _rollingCountOfMovesInEachTurnAcrossGames = new Dictionary<int, int>();

    //logger and random seed
    private SeededRandom _intRng; //this is used for random integers
    private Random _doubleRng;// this is used for random doubles
    private Logger _log;
    private bool _outputTreeStructureToLogFile = false;
    private int _noIterationsPerLog = 100; //how often to output tree structure as we progress through MC loop
    private bool _logChosenMove = false;
    string _logFilePath = "SOIMCTS_Tree_Log.txt";
    
    //boolean to track start of turn 
    private bool _startOfTurn = true;
    
    //counters to track stats for a single game
    private int _simsCounter; // total number of MCTS simulations for a single game 
    private int _turnCounter; // total number of turns for a single game
    private int _moveCounter; // total number of moves for a single game
    private int _movesWithSimsCounter; //total number of moves for a single game which use MCTS simulation loop
    private TimeSpan _totalTimeForGame; //time taken for this game
    
    //counters across multiple games
    private static int _gameCounter; // total number of games played in a session
    private static int _totalSimsCounter; //tracks total number of sims across all games played in a session
    
    //variables to track stats for a single move
    private static int _depthCounter ; //total final tree depth prior to choosing move
    private static int _widthAndDepthCalcCount; //used tp normalise values in _widthTreeLayers and to normalise _depthCounter
    private static List<int> _widthTreeLayers; //number of nodes in each layer of tree when choosing move (which are assuming we
    //dont go more than five levels deep)
    private static int _moveTimeOutCounter; //number of times we time out a move across a game
    
    //Taken from BestMCTS3 - as we reuse the same heuristic
    private GameStrategySOISMCTS _strategy = new(10, GamePhase.EarlyGame);
    
    private void PrepareForGame()
    { 
        //if any agent set-up needed it can be done here
        
        //seed random number generator
        long seed = DateTime.Now.Ticks;
        //long seed = 123;
        _intRng = new((ulong)seed); 
        _doubleRng = new Random((int) seed); 
        
        //create logger object
        _log = new Logger();
        _log.P1LoggerEnabled = true;
        _log.P2LoggerEnabled = true;
        
        //initialise start of turn and game bools
        _startOfTurn = true;
        
        //initialise counters
        _turnCounter = 0;
        _moveCounter = 0;
        _movesWithSimsCounter = 0;
        _simsCounter = 0;
        _depthCounter = 0;
        _widthTreeLayers = Enumerable.Repeat(0, 15).ToList();
        _widthAndDepthCalcCount = 0;
        _moveTimeOutCounter = 0;
        
        //increment game counter
        _gameCounter += 1;
        
        //initialise timer for this game
        _totalTimeForGame = TimeSpan.FromSeconds(0);
        
        //at start of game reused tree node is set to null
        _reusedRootNode = null;
        
        //initialise time for move computation to a fixed time if we are not allocating time by turn
        if (!_timeAllocation)
        {
            _timeForMoveComputation = TimeSpan.FromSeconds(0.55);
        }
    }

    public SOISMCTS()
    {
        this.PrepareForGame();
    }
    
    public override PatronId SelectPatron(List<PatronId> availablePatrons, int round)
    {
        return availablePatrons.PickRandom(_intRng);
    }

    public override Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime)
    {
        if (_startOfTurn)
        {
            _usedTimeInTurn = TimeSpan.FromSeconds(0);
            SelectStrategy(gameState);
            
            //we maintain MAST stats across moves within a turn
            mastStats = new Dictionary<MASTMoveKey, (double totalReward, int count)>();
            
            if (_timeAllocation)
            {
                //in this case we allocate time for this move based on the expected number of moves this turn
                double expNoMovesForThisTurn =
                    _expNoMovesPerTurn.TryGetValue(_turnCounter, out double noMoves) ? noMoves : 1;
                _timeForMoveComputation = _turnTimeout / (1.0 * (expNoMovesForThisTurn));
            }
        }
        
        //if only possible move is end turn then just end the turn
        if (possibleMoves.Count == 1 && possibleMoves[0].Command == CommandEnum.END_TURN)
        {
            _startOfTurn = true;
            _moveCounter += 1;
            //_rollingCountOfMovesInEachTurnAcrossGames[_turnCounter] 
            //    = _rollingCountOfMovesInEachTurnAcrossGames.TryGetValue(_turnCounter, out int count1) ? count1 + 1 : 1;
            _turnCounter += 1;
            return possibleMoves[0];
        }
        
        //Initialise a root node
        SeededGameState s = gameState.ToSeededGameState((ulong) _intRng.Next());
        List<Move> filteredMoves = FilterMoves(possibleMoves, s);
        Determinisation d = new Determinisation(s, filteredMoves);
        InfosetNode? root = null;
        if (_treeReuse && !_startOfTurn && _reusedRootNode != null)
        {
            root = _reusedRootNode;
            root.SetCurrentDeterminisationAndMoveHistory(d, null);   
        }
        else
        {
            root = new InfosetNode(null, null, d);
        }
        _reusedRootNode = null; 
        _startOfTurn = false;
        
        //check to see if we only have one move available after filtering moves and if so just return that
        if (filteredMoves.Count == 1)
        {
            if (_treeReuse)
            {
                //TODO::do we need to do anything here? Can we get here with a previous node that is null?
                //since we expect as soon as we get a choice all future moves will have a choice, until they dont
                //but shouldnt have the situation where we have one filtered moves repeatedly, then a choice, then
                //only one filtered move afterwards......
            }
            _moveCounter += 1;
            //_rollingCountOfMovesInEachTurnAcrossGames[_turnCounter] 
             //   = _rollingCountOfMovesInEachTurnAcrossGames.TryGetValue(_turnCounter, out int count2) ? count2 + 1 : 1;
            return filteredMoves[0];
        }
        
        Move chosenMove = null;
        InfosetNode nextNode = null;
        if (_usedTimeInTurn + _timeForMoveComputation >= _turnTimeout)
        {
            _moveTimeOutCounter += 1;
            //if we run out of time for this move, don't choose end turn
            chosenMove = NotEndTurnPossibleMoves(filteredMoves).PickRandom(_intRng);
        }
        else
        {
            int maxDepthForThisMove = 0;
            Stopwatch timer = new Stopwatch(); //why does this not have an issue with multi-threading?
            timer.Start();
            int noIterations = 1;
            while (timer.Elapsed < _timeForMoveComputation)
            //int maxIterations = 10000;
            //for(int i = 1; i <= maxIterations; i++)
            {
                //in InfosetMCTS each iteration of the loop starts with a new determinisation, which we use to explore the same tree
                //updating the stats for each tree node
                if (_randomDeterminisations)
                {
                    if (noIterations % _noSimsPerRandomisation == 0)
                    {
                        s = gameState.ToSeededGameState((ulong)_intRng.Next());
                        filteredMoves = FilterMoves(possibleMoves, s);
                        d = new Determinisation(s, filteredMoves);
                        //and set as determinisation to use for this iteration
                        root.SetCurrentDeterminisationAndMoveHistory(d, null);
                    }
                }
                
                //enter selection routine - return an array of nodes with index zero corresponding to root and final
                //entry corresponding to the node selected for expansion
                var(pathThroughTree, cvd, uvd) = select(root);

                //if selected node has moves leading to nodes not in the tree then expand the tree
                InfosetNode selectedNode = pathThroughTree[pathThroughTree.Count -1];
                InfosetNode expandedNode = selectedNode;
                
                //dont expand an end_turn node
                if (!selectedNode._endTurn)
                {
                    if (uvd.Count != 0)
                    {
                        expandedNode = Expand(selectedNode, uvd, pathThroughTree);
                    }
                    else
                    {
                        //this should never happen unless we are in a game end state
                    }
                }

                //next we simulate our playouts from our expanded node 
                double payoutFromExpandedNode = Rollout(expandedNode);

                //next we complete the backpropagation step
                BackPropagation(payoutFromExpandedNode, pathThroughTree);
                
                if (_outputTreeStructureToLogFile && (noIterations % _noIterationsPerLog == 0))
                {
                    using (StreamWriter logWriter = new StreamWriter(_logFilePath, true))  // Append mode
                    {
                        logWriter.WriteLine("Iteration number:" + noIterations.ToString());
                        logWriter.WriteLine("Turn number:" + _turnCounter.ToString());
                        logWriter.WriteLine("Move number:" + _moveCounter.ToString());
                        TreeReporterSOISMCTS.ReportTreeStructure(root, K, logWriter);
                        logWriter.WriteLine();
                    }
                }
                
                //update counters and max depth found ofr this iteration
                _simsCounter += 1;
                _totalSimsCounter += 1;
                maxDepthForThisMove = Math.Max(maxDepthForThisMove, pathThroughTree.Count);
                noIterations += 1;
            }
            _usedTimeInTurn += _timeForMoveComputation;
            
            //increase depth counter
            _depthCounter += maxDepthForThisMove;
            
            //determine final width and depth of tree
            Dictionary<int,int> treeWidthForEachLayerForThisMove = InfosetNode.CalculateLayerSpans(root);
            for (int i = 0; i < maxDepthForThisMove; i++)
            {
                _widthTreeLayers[i] += treeWidthForEachLayerForThisMove[i];
            }
            //increase width calculator counter used to normalise previous counts
            _widthAndDepthCalcCount += 1;
            //this move used simulation (as opposed to just being from a choice of one move)
            _movesWithSimsCounter += 1;


            //finally we return the move from the root node that corresponds ot the best move
            (chosenMove, nextNode) = chooseBestMove(root);

            //output chosen move to log file
            if (_logChosenMove)
            {
                using (StreamWriter logWriter = new StreamWriter(_logFilePath, true)) // Append mode
                {
                    logWriter.WriteLine("Chosen Move: " + chosenMove.ToString());
                }
            }
        }
        
        //_rollingCountOfMovesInEachTurnAcrossGames[_turnCounter] 
        //    = _rollingCountOfMovesInEachTurnAcrossGames.TryGetValue(_turnCounter, out int count3) ? count3 + 1 : 1;
        
        //set-up root node for next move
        if (_treeReuse)
        {
            _reusedRootNode = prepareRootNodeForNextIteration(chosenMove, nextNode);
        }
        
        if (chosenMove.Command == CommandEnum.END_TURN)
        {
            _startOfTurn = true;
            _turnCounter += 1;
            _usedTimeInTurn = TimeSpan.FromSeconds(0);
        }
        
        _moveCounter += 1;
        
        return chosenMove;
    }

    public InfosetNode prepareRootNodeForNextIteration(Move move, InfosetNode node)
    {
        //to prepare a node for re-use in the next iteration we need to do the following things
        //1. Set it's parent to null
        //2. Remove the move used ot reach this node from it's move history and all it's children's move histories.
        //Note current determinisations can be ignored as tehyw ill be reset in the next iteration
        node.Parent = null; //clear parent
        int layer = node.GetRefMoveHistoryLength();
        node.clearRefHistory();
        node.removeMovesFromChildrenMoveHistory(layer); //clear reference move history as this node has now become the root.
        
        return node;
    }
    
    //returns the selected path through the tree, and also the children in tree and move not in tree for the final selected node
    public (List<InfosetNode>, HashSet<InfosetNode>, List<Move>) select(InfosetNode startNode)
    { 
        //descend our infoset tree (restricted to nodes/actions compatible with the current determinisation of our start node)
        //Each successive node is chosen based on the UCB score, until a node is reached such that all moves from that node are not in the tree
        //or that node is terminal
        InfosetNode bestNode = startNode;
        var (cvd, uvd) = startNode.calcChildrenInTreeAndMovesNotInTree();
        //this contains each node passed through in this iteration 
        List<InfosetNode> pathThroughTree = new List<InfosetNode>();
        pathThroughTree.Add(startNode);
        while (bestNode.GetCurrentDeterminisation().GetState().GameEndState == null && uvd.Count == 0)
        {
            double bestVal = double.NegativeInfinity;
            foreach(InfosetNode node in cvd)
            {
                double val = node.UCB(K);
                if (val > bestVal)
                {
                    bestVal = val;
                    bestNode = node;
                }
            }

            (cvd, uvd) = bestNode.calcChildrenInTreeAndMovesNotInTree();
            pathThroughTree.Add(bestNode);

            //dont continue to select past an end_turn node
            if (bestNode._endTurn)
            {
                break;
            }
        }
        return (pathThroughTree, cvd, uvd);
    }
    
    private InfosetNode Expand(InfosetNode selectedNode, List<Move> selectedUVD, List<InfosetNode> pathThroughTree)
    {
        //choose a move at random from our list of moves that do not have nodes in the tree
        //and add child node to tree
        //List<Move> uvd = selectedNode.GetMovesWithNoChildren();
        Move? move = null;
        InfosetNode? newNode = null;
        if (selectedUVD.Count >= 1)
        {
            if (_useMAST)
            {
                //bias tree expansion using Gibbs distribution and MAST stats
                move = selectNextMoveUsingMAST(selectedUVD);
            }
            else
            {
                move = selectedUVD.PickRandom(_intRng);
            }
            var (newSeededGameState, newMoves) = selectedNode.GetCurrentDeterminisation().GetState().ApplyMove(move);
            List<Move> newFilteredMoves = FilterMoves(newMoves, newSeededGameState); //TODO:Should we be filtering here?
            Determinisation newd = new Determinisation(newSeededGameState, newFilteredMoves);
            newNode = selectedNode.CreateChild(move, newd);
            pathThroughTree.Add(newNode);
            
            //next we add this new node to the parent's list of compatible children for this iteration, and also 
            //remove the move that generated this node from the list of moves that dont have a child in the tree. 
            //This means we dont need to call calcChildrenInTreeAndMovesNotInTree during the back propagation function
            selectedNode._compatibleChildrenInTree.Add(newNode);
            selectedNode._currentMovesWithNoChildren.Remove(move);
        }
        else
        {
            //Here we are trying to expand when all moves from the selected node have children already in the tree. 
            //This shouldn't be possible if our select function is working correctly
            throw new Exception("Error in expansion");
        }
        //return new infosetNode object, corresponding to the selected node
        return newNode;
    }

    //simulate our game from a given determinisation associated with our expanded node (ignoring information sets)
    //adapted from last years winner
    public double Rollout(InfosetNode startNode)
    {
        //if the move from the parent is END_TURN, we need to just take the heuristic value for the parent (end turn
        //doesnt change the value of player's position, and we cant apply the heuristic when the current player is the enemy 
        //player)
        if (startNode._endTurn)
        {
            if (_useMCTSBotHeuristic)
            {
                return HeuristicFromMCTSBot(startNode.Parent.GetCurrentDeterminisation().GetState());
            }
            else
            {
                return _strategy.Heuristic(startNode.Parent.GetCurrentDeterminisation().GetState());
            }
        }
        
        //used to update MAST stats
        List<Move> visitedMoves = new List<Move>();
        
        SeededGameState gameState = startNode.GetCurrentDeterminisation().GetState();
        //check that only move from startNode isn't an end turn
        List<Move> possibleMoves = startNode.GetCurrentDeterminisation().GetMoves();
        double finalPayOff = 0;
        List<Move> notEndMoves = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
        if (notEndMoves.Count == 0)
        {
            if (_useMCTSBotHeuristic)
            {
                return HeuristicFromMCTSBot(gameState);
            }
            else
            {
                return _strategy.Heuristic(gameState);
            }
        }

        Move move = null;
        if (_useMAST)
        {
            move = selectNextMoveUsingMAST(notEndMoves);
            visitedMoves.Add(move);
        }
        else
        {
            move = notEndMoves.PickRandom(_intRng);
        }
    
        while (move.Command != CommandEnum.END_TURN)
        {
            (gameState, possibleMoves) = gameState.ApplyMove(move);
            notEndMoves = possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
    
            if (notEndMoves.Count > 0)
            {
                if (_useMAST)
                {
                    move = selectNextMoveUsingMAST(notEndMoves);
                    visitedMoves.Add(move);
                }
                else
                {
                    move = notEndMoves.PickRandom(_intRng);
                }
            }
            else
            {
                move = Move.EndTurn();
            }
        }
        
        double payoff = 0;
        if (_useMCTSBotHeuristic)
        {
            payoff = HeuristicFromMCTSBot(gameState);
        }
        else
        {
            payoff = _strategy.Heuristic(gameState);
        }
        
        if (_useMAST)
        {
            foreach (Move mv in visitedMoves)
            {
                MASTMoveKey key = new MASTMoveKey(mv);
                if (!mastStats.ContainsKey(key))
                {
                    mastStats[key] = (0, 0);
                }
                var (totalReward, count) = mastStats[key];
                mastStats[key] = (totalReward + payoff, count + 1);
            }
        }
        
        return payoff;
    }

    private Move selectNextMoveUsingMAST(List<Move> moveList)
    {
        if (moveList.Count == 1)
        {
            return moveList[0];
        }
        else if (_doubleRng.NextDouble() < _thresholdForRandomMoves)
        {
            return moveList.PickRandom(_intRng);
        }
        else
        {
            List<double> moveProbabilities = Enumerable.Repeat(0.0, moveList.Count()).ToList();
            List<MASTMoveKey> moveKeys = new List<MASTMoveKey>();
            double totalSum = 0.0;

            //note, need ot be careful here as we may have multiple moves of the same type (e.g. play gold which will only
            //have one entry in mastStats, and also will have duplicate MASTMoveKeys
            for(int i = 0; i < moveList.Count; i++)
            {
                moveKeys.Add(new MASTMoveKey(moveList[i]));
                double averageReward =
                    mastStats.TryGetValue(moveKeys[i], out var stats) ? stats.totalReward / stats.count : 0.0;
                double probability = Math.Exp(averageReward / _temperature);
                moveProbabilities[i] = probability;
                totalSum += probability;
            }

            // Ensure we have non-zero totalSum to avoid division by zero
            if (totalSum < 0.00000000001)
            {
                // If totalSum is zero, all probabilities are zero; fall back to uniform distribution
                for(int i = 0; i < moveProbabilities.Count; i++)
                {
                    moveProbabilities[i] = 1.0 / moveList.Count;
                }
            }
            else
            {
                // Normalize the probabilities
                for(int i = 0; i < moveProbabilities.Count; i++)
                {
                    moveProbabilities[i] /= totalSum;
                }
            }

            double cumulativeProbability = 0.0;
            double randomValue = _doubleRng.NextDouble();
            for(int i = 0; i < moveList.Count; i++)
            {
                cumulativeProbability += moveProbabilities[i];
                if (randomValue <= cumulativeProbability)
                {
                    return moveKeys[i].GetMove();
                }
            }
        }

        //this shouldn't happen
        return null;
    }

    //function to backpropagate simulation playout results
    private void BackPropagation(double finalPayout, List<InfosetNode> pathThroughTree)
    {
        //need to traverse tree from the playout start node back up to the root of the tree
        //note that our path through the tree holds references to tree nodes and hence can be updated directly
        //note that we count from an index of one so that we dont update stats in the root node
        for(int i = 1; i < pathThroughTree.Count; i++)
        {
            InfosetNode node = pathThroughTree[i];
            node.VisitCount += 1;
            node.TotalReward += finalPayout;
            node.MaxReward = Math.Max(finalPayout, node.MaxReward);
            node.AvailabilityCount += 1;
            //update averge reward and UCBExploration values
            node.UCB(K);
            //by definition the final node in the path through the tree wont have any compatible children in the tree
            //to see this there are two case, 1. where selected node has uvd.count =0 (and the expanded node is the same as the selected node
            //, in which case we should be in an end game state and there are no further children to include in the tree
            //or 2. uvd.count is not zero for the sected node, and the expanded node has just been added into the tree in which case
            //again we have no compatible children in the tree
            if (i < (pathThroughTree.Count - 1))
            {
                foreach (InfosetNode child in node._compatibleChildrenInTree)
                {
                    child.AvailabilityCount += 1;
                }
            }
        }
    }
    
    //chooses child from root node with highest visitation number, returns that node and the move to get to that node
    public (Move, InfosetNode) chooseBestMove(InfosetNode rootNode)
    {
        double bestScore = double.NegativeInfinity;
        Move bestMove = null;
        InfosetNode bestNode = null;
        //Note if we re-use a node, it may have children which are not in our list of filtered moves for this move.
        //for example if our only possible first move left after filtering is war song, and this is played the next layer down
        //might consist of say 4 children corresponding to play gold, way of the sword, end_turn and fortify.
        //then if we filter our moves so that only fortify is available then only one of these children is available.
        //Our current determinisation for the root (and root only) reflects our filtering and is a constant across all our
        //determinisations (across all iterations), and hence we should choose from compatible children for our root node.
        //As a final comment, if we are not re-using our node the chice between using children vs compatibleChildrenInTree
        //can still have an impact, due to differing ordering of nodes and if two nodes have the same maxreward
        //then one that comes first will be chosen.
        //foreach (InfosetNode node in rootNode.Children) 
        foreach (InfosetNode node in rootNode._compatibleChildrenInTree)
        {
            if (_useMaxReward)
            {
                if(node.MaxReward >= bestScore)
                {
                    bestScore = node.MaxReward;
                    bestMove = node.GetCurrentMoveFromParent();
                    bestNode = node;
                }  
                // double score = node.TotalReward / (1.0 * node.VisitCount);
                // if(score >= bestScore)
                // {
                //     bestScore = score;
                //     bestMove = node.GetCurrentMoveFromParent();
                //     bestNode = node;
                // }  
            }
            else
            {
                if(node.VisitCount >= bestScore)
                {
                    bestScore = node.VisitCount;
                    bestMove = node.GetCurrentMoveFromParent();
                    bestNode = node;
                }  
            }
       
        }
        
        return (bestMove, bestNode);
    }
    
    //taken from MCTSBot
    private List<Move> NotEndTurnPossibleMoves(List<Move> possibleMoves)
    {
         return possibleMoves.Where(m => m.Command != CommandEnum.END_TURN).ToList();
    }
    
     //taken from BestMCTS3
    private List<Move> FilterMoves(List<Move> moves, SeededGameState gameState)
    {
        if (!_filterMoves)
            return moves;
        
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
                        choices.Add((mv, _strategy.CardEvaluation(mcm!.Choices[0], gameState)));
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
                            val += _strategy.CardEvaluation(card, gameState);
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
    
    //taken from previous years winner
    void SelectStrategy(GameState gameState)
    {
        var currentPlayer = gameState.CurrentPlayer;
        int cardCount = currentPlayer.Hand.Count + currentPlayer.CooldownPile.Count + currentPlayer.DrawPile.Count;
        int points = gameState.CurrentPlayer.Prestige;
        if (points >= 27 || gameState.EnemyPlayer.Prestige >= 30)
        {
            _strategy = new GameStrategySOISMCTS(cardCount, GamePhase.LateGame);
        }
        else if (points <= 10 && gameState.EnemyPlayer.Prestige <= 13)
        {
            _strategy = new GameStrategySOISMCTS(cardCount, GamePhase.EarlyGame);
        }
        else
        {
            _strategy = new GameStrategySOISMCTS(cardCount, GamePhase.MidGame);
        }
    }
    
    public double HeuristicFromMCTSBot(SeededGameState gameState)
    {
        int patronFavour = 50;
        int patronNeutral = 10;
        int patronUnfavour = -50;
        int coinsValue = 1;
        int powerValue = 40;
        int prestigeValue = 50;
        int agentOnBoardValue = 30;
        int hpValue = 3;
        int opponentAgentsPenaltyValue = 40;
        int potentialComboValue = 3;
        int cardValue = 10;
        int penaltyForHighTierInTavern = 2;
        int numberOfDrawsValue = 10;
        int enemyPotentialComboPenalty = 1;
    
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
        //finalValue += gameState.CurrentPlayer.Coins * coinsValue;

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
            List<UniqueCard> allCardsEnemy = gameState.EnemyPlayer.Hand.Concat(gameState.EnemyPlayer.DrawPile).Concat(gameState.EnemyPlayer.Played.Concat(gameState.EnemyPlayer.CooldownPile)).ToList();
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

        //int finalValue = gameState.CurrentPlayer.Power + gameState.CurrentPlayer.Prestige;
        double normalizedValue = NormalizeHeuristic(finalValue);

        return normalizedValue;
    }

    private double NormalizeHeuristic(int value)
    {
        int heuristicMax = 40000; //160
        int heuristicMin = -10000;//00
    
        double normalizedValue = ((double)value - (double)heuristicMin) / ((double)heuristicMax - (double)heuristicMin);

        if (normalizedValue < 0)
        {
            return 0.0;
        }

        return normalizedValue;
    }
    
    public override void GameEnd(EndGameState state, FullGameState? finalBoardState)
    {
        double avgMovesPerTurn = _moveCounter/ (1.0 * _turnCounter);
        double avgSimsPerMove = _simsCounter / (1.0 * _movesWithSimsCounter);
        double avgDepthPerMove = _depthCounter/ (1.0 * _widthAndDepthCalcCount);
        double avgMoveTimeOutsPerTurn = _moveTimeOutCounter/(1.0 * _turnCounter);
        
        string message = "Game count: " + _gameCounter.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "Turn Counter: " + _turnCounter.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "Average number of moves per turn: " + avgMovesPerTurn.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "Average number of move timeouts per turn: " + avgMoveTimeOutsPerTurn.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "Average number of simulations per move: " + avgSimsPerMove.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "Average tree depth searched per move: " + avgDepthPerMove.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "Average widths of each layer of the tree per move: ";
        for (int i = 0; i < _widthTreeLayers.Count; i++)
        {
            message += (_widthTreeLayers[i]/ (1.0 * _widthAndDepthCalcCount)).ToString() + ",";
        }
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "Winner: " + state.Winner.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "Game end reason: " + state.Reason.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        int minutes = _totalTimeForGame.Minutes;
        int seconds = _totalTimeForGame.Seconds;
        //message = "Time taken by SOISMCTS bot for this game: " + $"Elapsed time: {minutes} minutes and {seconds} seconds.";
        //_log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        message = "total number of sims across all games: " + _totalSimsCounter.ToString();
        _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);

        // message = "Rolling total of moves per turn across game: " + DictionaryToString<int,int>(_rollingCountOfMovesInEachTurnAcrossGames);
        // _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        //
        // //calculate expected number of moves per turn
        // foreach (var entry in _rollingCountOfMovesInEachTurnAcrossGames)
        // {
        //     _expNoMovesPerTurn[entry.Key] = entry.Value/(1.0*_gameCounter);
        // }
        //
        // message = "Expected number of moves per turn: " + DictionaryToString<int,double>(_expNoMovesPerTurn);
        // _log.Log(finalBoardState.CurrentPlayer.PlayerID, message);
        //

        //prepare for next game
        this.PrepareForGame();
    }
    
    static string DictionaryToString<T1, T2>(Dictionary<T1, T2> dictionary)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var entry in dictionary)
        {
            sb.AppendFormat("{0}: {1}, ", entry.Key, entry.Value);
        }
        // Remove the trailing comma and space, and add the closing brace
        if (dictionary.Count > 0)
        {
            sb.Length -= 2; // Remove the last ", "
        }
        sb.Append(" }");
        return sb.ToString();
    }
}

//Each node corresponds to an information set for the observing player for a tree,
//and also contains the determinisation that was used when the node was last visited in the tree 
public class InfosetNode
{
    public InfosetNode? Parent; //parent node
    public HashSet<InfosetNode> Children; //list of all children that have been visited from this node irrespective of the determinisation
    public double TotalReward;
    public double MaxReward;
    public int VisitCount;
    public int AvailabilityCount;
    public double AvgReward;
    public double UCBExploration;
    
    //Next we have a reference move history that keeps it's value between MCTS iterations and changing determinisations,
    //Any nodes with identical reference move histories are ocnsidered ot be the same node
    public List<Move> _refMoveHistory;
    
    //member variables to keep track of current determinisation being used, which children are compatible with the current
    //detereminisation and are in the tree, also the list of moves used to get to this node based on the current determinisation.
    private Determinisation? _currentDeterminisation; //to store determinisation that is currently being used in MCTS
    private List<Move>? _currentMoveHistory; //stores the history of moves from the root to this node, based on the current determinisation (this also
    //includes the current move from parent as the final entry in the list)
    public List<Move>? _currentMovesWithNoChildren; //stores moves from node using current determinisation that have no children
    public HashSet<InfosetNode>? _compatibleChildrenInTree; //list of children of this node compatible with moves using current determinisation 
    
    //to label nodes which have been arrived at through an END_TURN command
    public bool _endTurn;
    
    //hash code based on reference move history that is used to search for the node
    private ulong _hashCode;
    
    public InfosetNode(InfosetNode? parent, Move? currentMoveFromParent, Determinisation d)
    {
        Parent = parent;
        Children = new HashSet<InfosetNode>();
        
        //initialise values for UCB calc
        TotalReward = 0;
        MaxReward = 0;
        VisitCount = 1;
        AvailabilityCount = 1;
        
        //initialise references to define equivalence classes for information sets
        _refMoveHistory = new List<Move>(); // we initialise ref move history using the determinisation being used at the time of the node's creation
        //(form parent). Then in future iterations of the MCTS loop, the reference move history will be used to assess if this node is then equivalent to another node with
        //some other move history based on a different determinisation. We dont take the parents reference move history, as this may have come form a different
        //determinisation than the parent move (would this make a difference?)
        if (parent is not null)
        {
            if (parent._currentMoveHistory is not null)
            {
                foreach (Move mv in parent._currentMoveHistory)
                {
                    _refMoveHistory.Add(mv);
                }
            }

            if (currentMoveFromParent is not null)
            {
                _refMoveHistory.Add(currentMoveFromParent);
            }
        }

        _endTurn = false;
        if ((currentMoveFromParent is not null) && currentMoveFromParent.Command == CommandEnum.END_TURN)
        {
            _endTurn = true;
        }
            
        //set current determinisation and current move history
        SetCurrentDeterminisationAndMoveHistory(d, _refMoveHistory);
        
        //set hashcode
        _hashCode = calcHashCode(_refMoveHistory);
    }

    //next three functions are used when preparing a node for re-use as a root node
    public int GetRefMoveHistoryLength()
    {
        return _refMoveHistory.Count;
    }
    public void clearRefHistory()
    {
        _refMoveHistory = new List<Move>();
    }
    
    public void removeMovesFromChildrenMoveHistory(int layer)
    {
        foreach (InfosetNode child in Children)
        {
            child._refMoveHistory.RemoveRange(0, layer);
            child.removeMovesFromChildrenMoveHistory(layer);
        }
    }
    private ulong calcHashCode(List<Move> moveHistory)
    {
        unchecked // Allow arithmetic overflow, which is fine in this context
        {
            ulong hash = 19;
            foreach (Move mv in moveHistory)
            {
                //note a move should only be null at the root, so the root node will have a hash code of 19
                if (mv is not null)
                {
                    hash = hash * 31 + MoveComparer.HashMove(mv);
                }
            }
            return hash;
        }
    }
    
    public override bool Equals(object obj)
    {
        if (obj == null || obj is not InfosetNode)
            return false;

        InfosetNode other = (InfosetNode) obj;

        return CheckEquivalence(other._refMoveHistory);
    }

    public override int GetHashCode()
    {
        //once reference move history is set when creating node, hash code is fixed.
        return (int)_hashCode;
    }
    
    //this method updates the current determinisation and move history for a node, also
    //for each of the nodes further down the tree sets to null the current determinisation
    //and move history, so that they get recalculated when moving down the tree with the current determinisation
    public void SetCurrentDeterminisationAndMoveHistory(Determinisation? d, List<Move>? currentMoveHistory)
    {
        //set current determinisation
        _currentDeterminisation = d;
        _currentMovesWithNoChildren = null; //will be calculated as needed
        _compatibleChildrenInTree = null; //will be calculated as needed
        if (currentMoveHistory is null)
        {
            _currentMoveHistory = new List<Move>();
        }
        else
        {
            _currentMoveHistory = currentMoveHistory;
        }
        
        //if this node has children then we need to clear the move histories and current determinisations from them,
        //so they can be set and recalculated as needed. This means that there will be children in the tree without
        //any current parent move, however they will still have a reference move history, with a move from the parent node to the child node.
        foreach (InfosetNode child in Children)
        {
            child.SetCurrentDeterminisationAndMoveHistory(null, null);
        }
    }

    public Move GetCurrentMoveFromParent()
    {
        if (_currentMoveHistory.Count != 0)
        {
            return _currentMoveHistory[_currentMoveHistory.Count - 1];
        }
        else
        {
            return null;
        }
        //return _currentMoveHistory[_currentMoveHistory.Count - 1];
    }
    
    public Determinisation? GetCurrentDeterminisation()
    {
        return _currentDeterminisation;
    }

    //calculate upper confidence bound for trees, bandit algorithm for MCTS tree policy
    public double UCB(double K)
    {
        AvgReward = TotalReward / (VisitCount * 1.0);
        UCBExploration = K * Math.Sqrt(Math.Log(AvailabilityCount) / (VisitCount * 1.0));
        double ucbVal = AvgReward + UCBExploration;
        return ucbVal;
    }
    
    //For current determinisation calculates compatible children in the tree
    //and list of moves for which there are no children
    public (HashSet<InfosetNode>, List<Move>) calcChildrenInTreeAndMovesNotInTree()
    {
        // Don't add children of an end turn node into the tree as we are then analyzing enemy player nodes also.
        if (_endTurn)
        {
            _compatibleChildrenInTree = new HashSet<InfosetNode>(); // never include children from an end turn in our tree
            _currentMovesWithNoChildren = null; // doesn't have any meaning when we are at end an end turn
            return (_compatibleChildrenInTree, _currentMovesWithNoChildren);
        }

        _compatibleChildrenInTree = new HashSet<InfosetNode>();
        _currentMovesWithNoChildren = new List<Move>();
        
        foreach (Move move in _currentDeterminisation.GetMoves())
        {
            // Create new state
            var (newState, newMoves) = _currentDeterminisation.GetState().ApplyMove(move);

            // Create move history to newState
            List<Move> moveHistoryToState = new List<Move>(_currentMoveHistory) { move };

            // Find if new state or move history is in tree or not
            bool foundInChildren = false;

            foreach (InfosetNode child in Children)
            {
                if (child.CheckEquivalence(moveHistoryToState))
                {
                    // Found child node that represents an information set containing equivalent states
                    foundInChildren = true;
                    child.SetCurrentDeterminisationAndMoveHistory(new Determinisation(newState, newMoves),
                        moveHistoryToState);

                    if (!_compatibleChildrenInTree.Contains(child))
                    {
                        _compatibleChildrenInTree.Add(child);
                    }

                    break; // No need to check other children if we found a match
                }
            }

            if (!foundInChildren)
            {
                _currentMovesWithNoChildren.Add(move);
            }
        }

        return (_compatibleChildrenInTree, _currentMovesWithNoChildren);
    }
    
    public InfosetNode CreateChild(Move? parentMove, Determinisation newd)
    {
        InfosetNode childNode = new InfosetNode(this, parentMove, newd);
        Children.Add(childNode);
        
        return childNode;
    }
    
    //checks to see if two nodes are equivalent, based on their reference move history
    private bool CheckEquivalence(List<Move>? moveHistory)
    {
        if (!checkMovesListAreEqual(this._refMoveHistory, moveHistory))
            return false;
        
        return true;
    }
    
    //simple function to check that lists of moves are the same
    private bool checkMovesListAreEqual(List<Move> list1, List<Move> list2)
    {
        if (list1.Count != list2.Count)
            return false;

        if (list1.Count == 0)
            return true;

        for (int index = 0; index < list1.Count; index++)
        {
            if (!MoveComparer.AreIsomorphic(list1[index], list2[index]))
                return false;
        }

        return true;
    }
    
    //function to compute width of tree at each level
    public static Dictionary<int, int> CalculateLayerSpans(InfosetNode startNode)
    {
        Dictionary<int, int> layerSpans = new Dictionary<int, int>();
        CalculateLayerSpansRecursive(startNode, 0, layerSpans);
        return layerSpans;
    }

    private static void CalculateLayerSpansRecursive(InfosetNode node, int layer, Dictionary<int, int> layerSpans)
    {
        if (node == null) return;

        // Increment the count of nodes at this layer
        if (layerSpans.ContainsKey(layer))
        {
            layerSpans[layer]++;
        }
        else
        {
            layerSpans[layer] = 1;
        }

        // Recur for each child
        foreach (var child in node.Children)
        {
            CalculateLayerSpansRecursive(child, layer + 1, layerSpans);
        }
    }
}

//struct to encapsulate a specific determinisation, which includes a concrete game state and compatible moves
public class Determinisation
{
    private SeededGameState? state;
    private List<Move>? moves;

    public Determinisation(SeededGameState? gamestate, List<Move>? compatibleMoves)
    {
        state = gamestate;
        moves = compatibleMoves;
    }

    public SeededGameState? GetState()
    {
        return state;
    }
    
    public List<Move>? GetMoves()
    {
        return moves;
    }
}

//used as key for dictionary used ot store MAST stats. Cant use an ordinary move object as the hashcode is based only on COMMAND
//and hence move type
public class MASTMoveKey
{
    private Move _mv;
    private int _hashCode;
    
    public MASTMoveKey(Move mv)
    {
        _mv = mv;
        _hashCode = (int) MoveComparer.HashMove(mv);
    }
    
    public override bool Equals(object obj)
    {
        if (obj is not MASTMoveKey)
        {
            return false;
        }

        return this._hashCode == ((MASTMoveKey) obj).GetHashCode();
    }

    public override int GetHashCode()
    {
        return _hashCode;
    }

    public Move GetMove()
    {
        return _mv;
    }
}

