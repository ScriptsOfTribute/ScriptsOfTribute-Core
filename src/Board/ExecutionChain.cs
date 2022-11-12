namespace TalesOfTribute;

public class ExecutionChain
{
    public delegate void OnComplete();

    public readonly Queue<Func<Player, Player, Tavern, PlayResult>> _chain = new();
    private PlayResult? _current;
    private OnComplete? _onComplete;

    private Player _owner;
    private Player _enemy;
    private Tavern _tavern;

    public ExecutionChain(Player owner, Player enemy, Tavern tavern)
    {
        _owner = owner;
        _enemy = enemy;
        _tavern = tavern;
    }

    public void AddCompleteCallback(OnComplete onComplete)
    {
        _onComplete = onComplete;
    }

    public void Add(Func<Player, Player, Tavern, PlayResult> func)
    {
        _chain.Enqueue(func);
    }

    public IEnumerable<PlayResult> Consume()
    {
        if (_current == null)
        {
            _current = _chain.Dequeue().Invoke(_owner, _enemy, _tavern);
            yield return _current;
        }

        if (!_current.Completed)
        {
            throw new Exception("Complete pending events before consuming further!");
        }

        while (_chain.Count > 0)
        {
            _current = _chain.Dequeue().Invoke(_owner, _enemy, _tavern);
            yield return _current;
        }

        _onComplete?.Invoke();
    }
}
