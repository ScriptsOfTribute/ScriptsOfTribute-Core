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

            var current = _chain.Dequeue().Invoke(owner, enemy, tavern);
            _pendingEffects.RemoveAt(0);
            if (current is BaseChoice c)
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

    public void MakeChoice<T>(List<T> choices, ComplexEffectExecutor executor) where T : IChoosable
    {
        if (PendingChoice is null)
        {
            throw new Exception("Pending choice is missing or wrong type.");
        }

        var result = executor.Enact(PendingChoice.ChoiceFollowUp, choices.Select(c => (IChoosable)c).ToList());

        PendingChoice = result switch
        {
            BaseChoice choice => choice,
            Failure f => throw new Exception(f.Reason),
            _ => null
        };
    }

    public static ExecutionChain FromEffects(List<BaseEffect> effects, BaseChoice? pendingChoice)
    {
        var chain = new ExecutionChain();
        effects.ForEach(e => chain.Add(e));
        chain.PendingChoice = pendingChoice;
        return chain;
    }
}
