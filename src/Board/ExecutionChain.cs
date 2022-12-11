namespace TalesOfTribute;

public class ExecutionChain
{
    public delegate void OnComplete();

    private readonly Queue<Func<IPlayer, IPlayer, ITavern, PlayResult>> _chain = new();
    private PlayResult? _current;
    private OnComplete? _onComplete;

    private IPlayer _owner;
    private IPlayer _enemy;
    private ITavern _tavern;

    public bool Empty => _chain.Count == 0;

    public ExecutionChain(IPlayer owner, IPlayer enemy, ITavern tavern)
    {
        _owner = owner;
        _enemy = enemy;
        _tavern = tavern;
    }

    public void AddCompleteCallback(OnComplete onComplete)
    {
        _onComplete += onComplete;
    }

    public void Add(Func<IPlayer, IPlayer, ITavern, PlayResult> func)
    {
        _chain.Enqueue(func);
    }

    public IEnumerable<PlayResult> Consume()
    {
        if (_chain.Count == 0)
        {
            _onComplete?.Invoke();
            yield break;
        }

        while (!Empty)
        {
            _current = _chain.Dequeue().Invoke(_owner, _enemy, _tavern);
            yield return _current;

            if (!_current.Completed)
            {
                throw new Exception("Complete pending events before consuming further!");
            }
        }

        _onComplete?.Invoke();
    }

    public void MergeWith(ExecutionChain other)
    {
        while (!other.Empty)
        {
            _chain.Enqueue(other._chain.Dequeue());
        }
        AddCompleteCallback(() => other._onComplete?.Invoke());
    }

    public static ExecutionChain Failed(string reason, IPlayer player, IPlayer enemy, ITavern tavern)
    {
        var result = new ExecutionChain(player, enemy, tavern);
        result.Add((_, _, _) => new Failure(reason));
        return result;
    }
}
