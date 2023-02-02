using ScriptsOfTribute.Board;
using ScriptsOfTribute.Board.CardAction;
using ScriptsOfTribute.Board.Cards;

namespace ScriptsOfTribute;

public class ExecutionChain
{
    private List<UniqueBaseEffect> _pendingEffects;
    public IReadOnlyCollection<UniqueBaseEffect> PendingEffects => _pendingEffects.AsReadOnly();
    public bool Empty => _pendingEffects.Count == 0;
    public Choice? PendingChoice { get; private set; }
    public bool Completed => Empty && PendingChoice is null;

    public ExecutionChain()
    {
        _pendingEffects = new();
    }

    public ExecutionChain(int capacity)
    {
        _pendingEffects = new List<UniqueBaseEffect>(capacity);
    }

    public void Add(UniqueBaseEffect effect)
    {
        _pendingEffects.Add(effect);
    }

    public IEnumerable<(PlayResult, IEnumerable<CompletedAction>)> Consume(IPlayer owner, IPlayer enemy, ITavern tavern)
    {
        if (PendingChoice is not null)
        {
            throw new EngineException("Complete pending events before consuming further!");
        }

        while (!Empty)
        {
            PendingChoice = null;

            var (current, completed) = _pendingEffects[0].Enact(owner, enemy, tavern);
            _pendingEffects.RemoveAt(0);
            if (current is Choice c)
            {
                PendingChoice = c;
            }

            yield return (current, completed);

            if (PendingChoice is not null)
            {
                throw new EngineException("Complete pending events before consuming further!");
            }
        }
    }

    public IEnumerable<CompletedAction> MakeChoice(List<UniqueCard> choices, ComplexEffectExecutor executor)
    {
        if (PendingChoice?.Type != Choice.DataType.CARD)
        {
            throw new EngineException("Pending choice is missing or wrong type.");
        }

        var (result, actions) = executor.Enact(PendingChoice, choices);

        PendingChoice = result switch
        {
            Choice choice => choice,
            Failure f => throw new EngineException(f.Reason),
            _ => null
        };

        return actions;
    }
    
    public IEnumerable<CompletedAction> MakeChoice(UniqueEffect choice, ComplexEffectExecutor executor)
    {
        if (PendingChoice?.Type != Choice.DataType.EFFECT)
        {
            throw new EngineException("Pending choice is missing or wrong type.");
        }

        var (result, actions) = executor.Enact(PendingChoice, choice);

        PendingChoice = result switch
        {
            Choice c => c,
            Failure f => throw new EngineException(f.Reason),
            _ => null
        };

        return actions;
    }

    public static ExecutionChain FromEffects(List<UniqueBaseEffect> effects, Choice? pendingChoice)
    {
        var chain = new ExecutionChain(effects.Count);
        effects.ForEach(e => chain.Add(e));
        chain.PendingChoice = pendingChoice;
        return chain;
    }
}
