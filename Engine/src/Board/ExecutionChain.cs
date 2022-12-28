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

    public IEnumerable<PlayResult> Consume(IPlayer owner, IPlayer enemy, ITavern tavern)
    {
        if (PendingChoice is not null)
        {
            throw new Exception("Complete pending events before consuming further!");
        }

        while (!Empty)
        {
            PendingChoice = null;

            var current = _pendingEffects[0].Enact(owner, enemy, tavern);
            _pendingEffects.RemoveAt(0);
            if (current is Choice c)
            {
                PendingChoice = c;
            }

            yield return current;

            if (PendingChoice is not null)
            {
                throw new Exception("Complete pending events before consuming further!");
            }
        }
    }

    public void MakeChoice(List<Card> choices, ComplexEffectExecutor executor)
    {
        if (PendingChoice?.Type != Choice.DataType.CARD)
        {
            throw new Exception("Pending choice is missing or wrong type.");
        }

        var result = executor.Enact(PendingChoice.FollowUp, choices);

        PendingChoice = result switch
        {
            Choice choice => choice,
            Failure f => throw new Exception(f.Reason),
            _ => null
        };
    }
    
    public void MakeChoice(Effect choice, ComplexEffectExecutor executor)
    {
        if (PendingChoice?.Type != Choice.DataType.EFFECT)
        {
            throw new Exception("Pending choice is missing or wrong type.");
        }

        var result = executor.Enact(PendingChoice.FollowUp, choice);

        PendingChoice = result switch
        {
            Choice c => c,
            Failure f => throw new Exception(f.Reason),
            _ => null
        };
    }

    public static ExecutionChain FromEffects(List<BaseEffect> effects, Choice? pendingChoice)
    {
        var chain = new ExecutionChain();
        effects.ForEach(e => chain.Add(e));
        chain.PendingChoice = pendingChoice;
        return chain;
    }
}
