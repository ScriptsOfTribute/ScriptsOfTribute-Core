using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;
using ScriptsOfTribute.utils;

namespace ScriptsOfTribute.Board;

public class ScriptsOfTributeApi : IScriptsOfTributeApi
{
    public ulong Seed { get; }
    public int TurnCount => _turnCount;
    public int TurnMoveCount => _turnMoveCount;
    public PlayerEnum CurrentPlayerId => _boardManager.CurrentPlayer.ID;
    public PlayerEnum EnemyPlayerId => _boardManager.EnemyPlayer.ID;
    public BoardState BoardState => _boardManager.CardActionManager.State;
    public SerializedChoice? PendingChoice => _boardManager.CardActionManager.PendingChoice?.Serialize();
    private EndGameState? _endGameState = null;

    private readonly BoardManager _boardManager;
    private int _turnCount = 1;
    private int _turnMoveCount = 1;
    public Logger Logger { get; } = new();

    // Constructors
    public ScriptsOfTributeApi(BoardManager boardManager)
    {
        _boardManager = boardManager;
    }

    public ScriptsOfTributeApi(PatronId[] patrons) : this(patrons, (ulong)Environment.TickCount)
    {
    }

    /// <summary>
    /// Initialize board with selected patrons. patrons argument should contain PatronId.TREASURY
    /// but it handles situation when user doesn't put it.
    /// </summary>
    public ScriptsOfTributeApi(PatronId[] patrons, ulong seed)
    {
        if (!Array.Exists(patrons, p => p == PatronId.TREASURY))
        {
            // In case user forgets about Treasury (she/he shouldn't)
            List<PatronId> tempList = patrons.ToList();
            tempList.Add(PatronId.TREASURY);
            patrons = tempList.ToArray();
        }
        _boardManager = new BoardManager(patrons, seed);
        _boardManager.SetUpGame();
        Seed = seed;
    }

    // Serialization
    public FullGameState GetFullGameState()
    {
        return _boardManager.SerializeBoard(_endGameState);
    }

    public EndGameState? MakeChoice(List<UniqueCard> choices)
        => Try(() => _boardManager.CardActionManager.MakeChoice(choices));

    public EndGameState? MakeChoice(UniqueEffect choice)
        => Try(() => _boardManager.CardActionManager.MakeChoice(choice));

    public SerializedPlayer GetPlayer(PlayerEnum playerId)
    {
        return new SerializedPlayer(
            playerId == CurrentPlayerId ? _boardManager.CurrentPlayer : _boardManager.EnemyPlayer
        );
    }

    public EndGameState? ActivateAgent(UniqueCard agent)
        => Try(() => _boardManager.ActivateAgent(agent));

    public EndGameState? AttackAgent(UniqueCard agent)
        => Try(() => _boardManager.AttackAgent(agent));

    // Patron related

    /// <summary>
    /// Activate Patron with patronId. Only CurrentPlayer can activate patron
    /// </summary>
    public EndGameState? PatronActivation(PatronId patronId) => Try(() => _boardManager.PatronCall(patronId));

        // cards related

    /// <summary>
    /// Buys card <c>card</c> in tavern for CurrentPlayer.
    /// Checks if CurrentPlayer has enough Coin and if no choice is pending.
    /// </summary>
    public EndGameState? BuyCard(UniqueCard card)
        => Try(() => _boardManager.BuyCard(card));

    /// <summary>
    /// Plays card <c>card</c> from hand for CurrentPlayer
    /// Checks if CurrentPlayer has this card in hand and if no choice is pending.
    /// </summary>
    public EndGameState? PlayCard(UniqueCard card)
        => Try(() => _boardManager.PlayCard(card));

    private EndGameState? Try(Action f)
    {
        if (_endGameState is not null)
        {
            return _endGameState;
        }

        try
        {
            _turnMoveCount += 1;
            f();
        }
        catch (EngineException e)
        {
            _endGameState = new EndGameState(EnemyPlayerId, GameEndReason.INCORRECT_MOVE, e.Message);
            return _endGameState;
        }
        catch (Exception e)
        {
            _endGameState = new EndGameState(EnemyPlayerId, GameEndReason.INTERNAL_ERROR, $"MESSAGE:\n{e.Message}\n\nSTACKTRACE:\n{e.StackTrace}\n\nSOURCE:{e.Source}\n");
            return _endGameState;
        }

        return null;
    }

    //others

    public List<Move> GetListOfPossibleMoves()
    {
        if (_endGameState is not null)
        {
            return new List<Move>();
        }

        var choice = _boardManager.CardActionManager.PendingChoice;
        switch (choice?.Type)
        {
            case Choice.DataType.CARD:
            {
                var result = new List<Move>();
                for (var i = choice.MinChoiceAmount; i <= choice.MaxChoiceAmount; i++)
                {
                    result.AddRange(choice.PossibleCards.GetCombinations(i).Select(Move.MakeChoice));
                }

                return result;
            }
            case Choice.DataType.EFFECT:
            {
                var result = new List<Move>();
                for (var i = choice.MinChoiceAmount; i <= choice.MaxChoiceAmount; i++)
                {
                    result.AddRange(choice.PossibleEffects.GetCombinations(i).Select(Move.MakeChoice));
                }

                return result;
            }
        }

        var currentPlayer = _boardManager.CurrentPlayer;
        var enemyPlayer = _boardManager.EnemyPlayer;
        var possibleMoves = currentPlayer
            .Hand
            .Select(Move.PlayCard)
            .Concat(from agent in currentPlayer.Agents
                where !agent.Activated
                select Move.ActivateAgent(agent.RepresentingCard))
            .ToList();

        var tauntAgents = enemyPlayer.Agents.FindAll(agent => agent.RepresentingCard.Taunt);
        if (currentPlayer.PowerAmount > 0)
        {
            possibleMoves.AddRange(tauntAgents.Any()
                ? tauntAgents.Select(agent => Move.Attack(agent.RepresentingCard))
                : enemyPlayer.Agents.Select(agent => Move.Attack(agent.RepresentingCard)));
        }

        if (currentPlayer.CoinsAmount > 0)
        {
            possibleMoves.AddRange(_boardManager.Tavern.GetAffordableCards(currentPlayer.CoinsAmount).Select(Move.BuyCard));
        }

        if (currentPlayer.PatronCalls > 0)
        {
            foreach (var patron in _boardManager.Patrons)
            {
                if (patron.CanPatronBeActivated(currentPlayer, enemyPlayer))
                {
                    possibleMoves.Add(Move.CallPatron(patron.PatronID));
                }
            }
        }

        possibleMoves.Add(Move.EndTurn());

        return possibleMoves;
    }

    public bool IsMoveLegal(Move playerMove)
    {
        List<Move> possibleMoves = GetListOfPossibleMoves(); // might be expensive

        return possibleMoves.Contains(playerMove);
    }

    // lack of general method that parse Move and does stuff

    public EndGameState? EndTurn()
    {
        _turnCount += 1;
        _turnMoveCount = 1;
        _boardManager.EndTurn();
        return CheckWinner();
    }

    /// <summary>
    /// Returns state of game after it had ended, including information like who won and in what way.
    /// If game is still going it returns null.
    /// </summary>
    public EndGameState? CheckWinner()
    {
        if (_endGameState is not null)
        {
            return _endGameState;
        }

        _endGameState = _boardManager.CheckAndGetWinner();
        return _endGameState;
    }

    public static IScriptsOfTributeApi FromSerializedBoard(FullGameState board)
    {
        return new ScriptsOfTributeApi(BoardManager.FromSerializedBoard(board));
    }

    public bool CanPatronBeActivated(PatronId patronId)
    {
        return _boardManager
            .Patrons.First(p => p.PatronID == patronId)
            .CanPatronBeActivated(_boardManager.CurrentPlayer, _boardManager.EnemyPlayer);
    }
}
