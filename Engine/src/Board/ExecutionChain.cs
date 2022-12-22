﻿using TalesOfTribute.Board.CardAction;

namespace TalesOfTribute;

public class ExecutionChain
{
    private List<BaseEffect> _pendingEffects = new();
    public IReadOnlyCollection<BaseEffect> PendingEffects => _pendingEffects.AsReadOnly();
    public bool Empty => _chain.Count == 0;
    public BaseChoice? PendingChoice { get; private set; }
    public bool Completed => Empty && PendingChoice is null;

    private readonly Queue<Func<IPlayer, IPlayer, ITavern, PlayResult>> _chain = new();
    private PlayResult? _current;

    public void Add(BaseEffect effect)
    {
        _chain.Enqueue(effect.Enact);
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

            _current = _chain.Dequeue().Invoke(owner, enemy, tavern);
            _pendingEffects.RemoveAt(0);
            if (_current is BaseChoice c)
            {
                PendingChoice = c;
            }

            yield return _current;

            if (PendingChoice is not null)
            {
                throw new Exception("Complete pending events before consuming further!");
            }
        }
    }

    public void MakeChoice<T>(List<T> choices, ComplexEffectExecutor executor)
    {
        if (PendingChoice is null || PendingChoice is not Choice<T> c)
        {
            throw new Exception("Pending choice is missing or wrong type.");
        }

        var result = c.Choose(choices, executor);

        PendingChoice = result switch
        {
            BaseChoice baseChoice => baseChoice,
            Failure f => throw new Exception(f.Reason),
            _ => null
        };
    }

    public void MergeWith(ExecutionChain other)
    {
        while (!other.Empty)
        {
            _chain.Enqueue(other._chain.Dequeue());
        }
    }
}