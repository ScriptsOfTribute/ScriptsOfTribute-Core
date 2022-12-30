using TalesOfTribute.Board.CardAction;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.Board;

public class TalesOfTributeApi : ITalesOfTributeApi
{
    public int TurnCount => _turnCount;
    public PlayerEnum CurrentPlayerId => _boardManager.CurrentPlayer.ID;
    public PlayerEnum EnemyPlayerId => _boardManager.EnemyPlayer.ID;
    public BoardState BoardState => _boardManager.CardActionManager.State;
    public SerializedChoice? PendingChoice => _boardManager.CardActionManager.PendingChoice?.Serialize();

    private readonly BoardManager _boardManager;
    private int _turnCount;

    // Constructors
    public TalesOfTributeApi(BoardManager boardManager)
    {
        _boardManager = boardManager;
    }

    /// <summary>
    /// Initialize board with selected patrons. patrons argument should contain PatronId.TREASURY
    /// but it handles situation when user doesn't put it.
    /// </summary>
    public TalesOfTributeApi(PatronId[] patrons)
    {
        if (!Array.Exists(patrons, p => p == PatronId.TREASURY))
        {
            // In case user forgets about Treasury (she/he shouldn't)
            List<PatronId> tempList = patrons.ToList();
            tempList.Add(PatronId.TREASURY);
            patrons = tempList.ToArray();
        }
        _boardManager = new BoardManager(patrons);
        _boardManager.SetUpGame();
    }

    // Serialization
    public SerializedBoard GetSerializer()
    {
        return _boardManager.SerializeBoard();
    }

    public void MakeChoice(List<Card> choices)
    {
        _boardManager.CardActionManager.MakeChoice(choices);
    }

    public void MakeChoice(Effect choice)
    {
        _boardManager.CardActionManager.MakeChoice(choice);
    }

    public SerializedPlayer GetPlayer(PlayerEnum playerId)
    {
        return new SerializedPlayer(
            playerId == CurrentPlayerId ? _boardManager.CurrentPlayer : _boardManager.EnemyPlayer
        );
    }

    public void ActivateAgent(Card agent)
        => _boardManager.ActivateAgent(agent);

    public ISimpleResult AttackAgent(Card agent)
        => _boardManager.AttackAgent(agent);

    public ISimpleResult AttackAgent(int uniqueId)
    {
        return _boardManager.AttackAgent(_boardManager.EnemyPlayer.GetCardByUniqueId(uniqueId));
    }

    // Patron related

    /// <summary>
    /// Activate Patron with patronId. Only CurrentPlayer can activate patron
    /// </summary>
    public void PatronActivation(PatronId patronId)
    {
        _boardManager.PatronCall(patronId);
    }

    // cards related

    /// <summary>
    /// Buys card <c>card</c> in tavern for CurrentPlayer.
    /// Checks if CurrentPlayer has enough Coin and if no choice is pending.
    /// </summary>
    public void BuyCard(Card card)
    {
        _boardManager.BuyCard(card);
    }

    /// <summary>
    /// Plays card <c>card</c> from hand for CurrentPlayer
    /// Checks if CurrentPlayer has this card in hand and if no choice is pending.
    /// </summary>
    public void PlayCard(Card card)
    {
        _boardManager.PlayCard(card);
    }

    //others

    public List<Move> GetListOfPossibleMoves()
    {
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

    public void EndTurn()
    {
        _turnCount++;
        _boardManager.EndTurn();
    }

    /// <summary>
    /// Returns state of game after it had ended, including information like who won and in what way.
    /// If game is still going it returns null.
    /// </summary>
    public EndGameState? CheckWinner()
    {
        return _boardManager.CheckAndGetWinner();
    }

    public static ITalesOfTributeApi FromSerializedBoard(SerializedBoard board)
    {
        return new TalesOfTributeApi(BoardManager.FromSerializedBoard(board));
    }

    public bool CanPatronBeActivated(PatronId patronId)
    {
        return _boardManager
            .Patrons.First(p => p.PatronID == patronId)
            .CanPatronBeActivated(_boardManager.CurrentPlayer, _boardManager.EnemyPlayer);
    }
}
