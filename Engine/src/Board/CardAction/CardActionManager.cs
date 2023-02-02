using ScriptsOfTribute.Board.Cards;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute.Board.CardAction;

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
    public List<UniqueBaseEffect> StartOfNextTurnEffects = new();
    private ComplexEffectExecutor _complexEffectExecutor;

    private Choice? _pendingPatronChoice;
    public Choice? PendingChoice => State == BoardState.PATRON_CHOICE_PENDING ? _pendingPatronChoice : _pendingExecutionChain.PendingChoice;
    public IReadOnlyCollection<UniqueBaseEffect> PendingEffects => _pendingExecutionChain.PendingEffects;
    public BoardState State { get; private set; } = BoardState.NORMAL;
    public List<CompletedAction> CompletedActions = new();

    public CardActionManager(IReadOnlyPlayerContext playerContext, ITavern tavern)
    {
        _playerContext = playerContext;
        _tavern = tavern;
        _pendingExecutionChain = new ExecutionChain();
        _complexEffectExecutor =
            new ComplexEffectExecutor(this, _playerContext.CurrentPlayer, _playerContext.EnemyPlayer, _tavern);
    }

    public void PlayCard(UniqueCard card)
    {
        if (State != BoardState.NORMAL)
        {
            throw new EngineException("Complete pending choices first!");
        }

        ImmediatePlayCard(card);
    }

    public void AddToCompletedActionsList(CompletedAction action)
    {
        CompletedActions.Add(action);
    }

    public void ImmediatePlayCard(UniqueCard card)
    {
        var (immediateEffects, startOfNextTurnEffects) = ComboContext.PlayCard(card);
        StartOfNextTurnEffects.AddRange(startOfNextTurnEffects);

        immediateEffects.ForEach(e => _pendingExecutionChain.Add(e));

        if (!ConsumePendingChainToChoice())
        {
            State = BoardState.CHOICE_PENDING;
        }
    }

    public void ActivatePatron(PlayerEnum player, Patron patron)
    {
        CompletedActions.Add(new CompletedAction(player, CompletedActionType.ACTIVATE_PATRON, patron.PatronID));
        var (result, actions) = patron.PatronActivation(_playerContext.CurrentPlayer, _playerContext.EnemyPlayer);
        CompletedActions.AddRange(actions);
        if (result is Choice c)
        {
            _pendingPatronChoice = c;
            State = BoardState.PATRON_CHOICE_PENDING;
        }
    }

    private void UpdatePendingPatronChoice(PlayResult result)
    {
        switch (result)
        {
            case Choice bc:
                _pendingPatronChoice = bc;
                break;
            case Failure f:
                throw new EngineException(f.Reason);
            default:
                State = BoardState.NORMAL;
                break;
        }
    }

    private void HandlePatronChoice(List<UniqueCard> choices)
    {
        if (_pendingPatronChoice?.Type != Choice.DataType.CARD)
        {
            throw new EngineException("MakeChoice of wrong type called.");
        }

        var (result, actions) = _complexEffectExecutor.Enact(_pendingPatronChoice, choices);
        CompletedActions.AddRange(actions);
        UpdatePendingPatronChoice(result);
    }
    
    private void HandlePatronChoice(UniqueEffect choice)
    {
        if (_pendingPatronChoice?.Type != Choice.DataType.EFFECT)
        {
            throw new EngineException("MakeChoice of wrong type called.");
        }

        var (result, actions) = _complexEffectExecutor.Enact(_pendingPatronChoice, choice);
        CompletedActions.AddRange(actions);
        UpdatePendingPatronChoice(result);
    }
    
    public void MakeChoice(List<UniqueCard> choices)
    {
        if (State == BoardState.NORMAL)
        {
            throw new EngineException("There is no pending choice.");
        }

        if (State == BoardState.PATRON_CHOICE_PENDING)
        {
            HandlePatronChoice(choices);
            return;
        }
        
        var actions = _pendingExecutionChain.MakeChoice(choices, _complexEffectExecutor);
        CompletedActions.AddRange(actions);

        if (ConsumePendingChainToChoice())
        {
            State = BoardState.NORMAL;
        }
    }
    
    public void MakeChoice(UniqueEffect choice)
    {
        if (State == BoardState.NORMAL)
        {
            throw new EngineException("There is no pending choice.");
        }

        if (State == BoardState.PATRON_CHOICE_PENDING)
        {
            HandlePatronChoice(choice);
            return;
        }
        
        var actions = _pendingExecutionChain.MakeChoice(choice, _complexEffectExecutor);
        CompletedActions.AddRange(actions);

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
            throw new EngineException("Something went wrong in the engine - not all choices are completed");
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

        foreach (var (result, actions) in _pendingExecutionChain.Consume(_playerContext.CurrentPlayer, _playerContext.EnemyPlayer, _tavern))
        {
            CompletedActions.AddRange(actions);
            if (result is Choice)
            {
                return false;
            }
        }

        return true;
    }

    public static CardActionManager FromSerializedBoard(FullGameState fullGameState, PlayerContext playerContext, ITavern tavern)
    {
        var comboContext = ComboContext.FromComboStates(fullGameState.ComboStates);

        Choice? choiceForChain = null;
        Choice? patronChoice = null;

        switch (fullGameState.BoardState)
        {
            case BoardState.NORMAL:
                break;
            case BoardState.CHOICE_PENDING:
            case BoardState.START_OF_TURN_CHOICE_PENDING:
                choiceForChain = fullGameState.PendingChoice!.ToChoice();
                break;
            case BoardState.PATRON_CHOICE_PENDING:
                patronChoice = fullGameState.PendingChoice!.ToChoice();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var chain = ExecutionChain.FromEffects(fullGameState.UpcomingEffects, choiceForChain);
        var startOfNextTurnEffects = new List<UniqueBaseEffect>(fullGameState.StartOfNextTurnEffects.Count);
        startOfNextTurnEffects.AddRange(fullGameState.StartOfNextTurnEffects);

        var completedActions = new List<CompletedAction>(fullGameState.CompletedActions.Count + 2);
        completedActions.AddRange(fullGameState.CompletedActions);

        var result = new CardActionManager(playerContext, tavern)
        {
            StartOfNextTurnEffects = startOfNextTurnEffects,
            State = fullGameState.BoardState,
            _pendingExecutionChain = chain,
            _pendingPatronChoice = patronChoice,
            ComboContext = comboContext,
            CompletedActions = completedActions,
        };

        return result;
    }
}
