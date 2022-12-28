using TalesOfTribute.Serializers;

namespace TalesOfTribute.Board.CardAction;

public enum BoardState
{
    NORMAL,
    CHOICE_PENDING,
    START_OF_TURN_CHOICE_PENDING,
    PATRON_CHOICE_PENDING,
}

public class CardActionManager
{
    private IReadOnlyPlayerContext _playerContext;
    private ITavern _tavern;
    private ExecutionChain _pendingExecutionChain;
    public ComboContext ComboContext { get; private set; } = new();
    public List<BaseEffect> StartOfNextTurnEffects = new();
    private ComplexEffectExecutor _complexEffectExecutor;

    private BaseChoice? _pendingPatronChoice;
    public BaseChoice? PendingChoice => State == BoardState.PATRON_CHOICE_PENDING ? _pendingPatronChoice : _pendingExecutionChain.PendingChoice;
    public IReadOnlyCollection<BaseEffect> PendingEffects => _pendingExecutionChain.PendingEffects;
    public BoardState State { get; private set; } = BoardState.NORMAL;

    public CardActionManager(IReadOnlyPlayerContext playerContext, ITavern tavern)
    {
        _playerContext = playerContext;
        _tavern = tavern;
        _pendingExecutionChain = new ExecutionChain();
        _complexEffectExecutor =
            new ComplexEffectExecutor(this, _playerContext.CurrentPlayer, _playerContext.EnemyPlayer, _tavern);
    }

    public void PlayCard(Card card)
    {
        if (State != BoardState.NORMAL)
        {
            throw new Exception("Complete pending choices first!");
        }

        ImmediatePlayCard(card);
    }

    public void ImmediatePlayCard(Card card)
    {
        var (immediateEffects, startOfNextTurnEffects) = ComboContext.PlayCard(card);
        StartOfNextTurnEffects.AddRange(startOfNextTurnEffects);

        immediateEffects.ForEach(e => _pendingExecutionChain.Add(e));

        if (!ConsumePendingChainToChoice())
        {
            State = BoardState.CHOICE_PENDING;
        }
    }

    public void ActivatePatron(Patron patron)
    {
        var result = patron.PatronActivation(_playerContext.CurrentPlayer, _playerContext.EnemyPlayer);
        if (result is BaseChoice c)
        {
            _pendingPatronChoice = c;
            State = BoardState.PATRON_CHOICE_PENDING;
        }
    }
    
    public void MakeChoice<T>(List<T> choices) where T : IChoosable
    {
        if (State == BoardState.NORMAL)
        {
            throw new Exception("There is no pending choice.");
        }

        if (State == BoardState.PATRON_CHOICE_PENDING)
        {
            if (_pendingPatronChoice is not Choice<T> c)
            {
                throw new Exception("MakeChoice of wrong type called.");
            }

            var result = _complexEffectExecutor.Enact(_pendingPatronChoice.ChoiceFollowUp, choices.Select(ch => (IChoosable)ch).ToList());
            switch (result)
            {
                case BaseChoice bc:
                    _pendingPatronChoice = bc;
                    break;
                case Failure f:
                    throw new Exception(f.Reason);
                default:
                    State = BoardState.NORMAL;
                    break;
            }

            return;
        }
        
        _pendingExecutionChain.MakeChoice(choices, _complexEffectExecutor);

        if (ConsumePendingChainToChoice())
        {
            State = BoardState.NORMAL;
        }
    }

    // Important: This must be called AFTER swapping players.
    public void Reset(IReadOnlyPlayerContext newPlayerContext)
    {
        if (State != BoardState.NORMAL || !_pendingExecutionChain.Completed)
        {
            throw new Exception("Something went wrong in the engine - not all choices are completed");
        }
        
        _playerContext = newPlayerContext;

        ComboContext.Reset();
        _complexEffectExecutor = new(this, _playerContext.CurrentPlayer, _playerContext.EnemyPlayer, _tavern);
        StartOfNextTurnEffects.ForEach(e =>
        {
            _pendingExecutionChain.Add(e);
        });
        StartOfNextTurnEffects.Clear();
        if (!ConsumePendingChainToChoice())
        {
            State = BoardState.START_OF_TURN_CHOICE_PENDING;
        }
    }

    private bool ConsumePendingChainToChoice()
    {
        if (PendingChoice is not null)
        {
            return false;
        }

        foreach (var result in _pendingExecutionChain.Consume(_playerContext.CurrentPlayer, _playerContext.EnemyPlayer, _tavern))
        {
            if (result is BaseChoice)
            {
                return false;
            }
        }

        return true;
    }

    public static CardActionManager FromSerializedBoard(SerializedBoard serializedBoard, PlayerContext playerContext, ITavern tavern)
    {
        // private ExecutionChain _pendingExecutionChain;
        var comboContext = ComboContext.FromComboStates(serializedBoard.ComboStates);

        BaseChoice? choiceForChain = null;
        BaseChoice? patronChoice = null;

        switch (serializedBoard.BoardState)
        {
            case BoardState.NORMAL:
                break;
            case BoardState.CHOICE_PENDING:
            case BoardState.START_OF_TURN_CHOICE_PENDING:
                choiceForChain = serializedBoard.PendingChoice!.ToChoice();
                break;
            case BoardState.PATRON_CHOICE_PENDING:
                patronChoice = serializedBoard.PendingChoice!.ToChoice();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        var chain = ExecutionChain.FromEffects(serializedBoard.UpcomingEffects, choiceForChain);
        var startOfNextTurnEffects = serializedBoard.StartOfNextTurnEffects.ToList();

        var result = new CardActionManager(playerContext, tavern)
        {
            StartOfNextTurnEffects = startOfNextTurnEffects,
            State = serializedBoard.BoardState,
            _pendingExecutionChain = chain,
            _pendingPatronChoice = patronChoice,
            ComboContext = comboContext,
        };

        return result;
    }
}
