using TalesOfTribute.Board;
using TalesOfTribute.Board.CardAction;

namespace TalesOfTribute;

public class ExecutionChain
{
    private List<BaseEffect> _pendingEffects = new();
    public IReadOnlyCollection<BaseEffect> PendingEffects => _pendingEffects.AsReadOnly();
    public bool Empty => _pendingEffects.Count == 0;
    public Choice? PendingChoice { get; private set; }
    public bool Completed => Empty && PendingChoice is null;

    public void Add(BaseEffect effect)
    {
        _pendingEffects.Add(effect);
    }

    public IEnumerable<(PlayResult, IEnumerable<CompletedAction>)> Consume(IPlayer owner, IPlayer enemy, ITavern tavern)
    {
        if (PendingChoice is not null)
        {
            throw new Exception("Complete pending events before consuming further!");
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
                throw new Exception("Complete pending events before consuming further!");
            }
        }
    }

    public IEnumerable<CompletedAction> MakeChoice(List<Card> choices, ComplexEffectExecutor executor)
    {
        if (PendingChoice?.Type != Choice.DataType.CARD)
        {
            throw new Exception("Pending choice is missing or wrong type.");
        }

        var (result, actions) = executor.Enact(PendingChoice.FollowUp, choices);

        PendingChoice = result switch
        {
            Choice choice => choice,
            Failure f => throw new Exception(f.Reason),
            _ => null
        };

        return actions;
    }
    
    public IEnumerable<CompletedAction> MakeChoice(Effect choice, ComplexEffectExecutor executor)
    {
        if (PendingChoice?.Type != Choice.DataType.EFFECT)
        {
            throw new Exception("Pending choice is missing or wrong type.");
        }

        var (result, actions) = executor.Enact(PendingChoice.FollowUp, choice);

        PendingChoice = result switch
        {
            Choice c => c,
            Failure f => throw new Exception(f.Reason),
            _ => null
        };

        return actions;
    }

    public static ExecutionChain FromEffects(List<BaseEffect> effects, Choice? pendingChoice)
    {
        var chain = new ExecutionChain();
        effects.ForEach(e => chain.Add(e));
        chain.PendingChoice = pendingChoice;
        return chain;
    }
}
