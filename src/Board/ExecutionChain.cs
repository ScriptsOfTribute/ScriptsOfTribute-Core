namespace TalesOfTribute;

public class ExecutionChain
{
    public delegate void OnComplete();

    private readonly Queue<Func<PlayResult>> _chain = new();
    private PlayResult? _current;
    private OnComplete? _onComplete;

    public void AddCompleteCallback(OnComplete onComplete)
    {
        _onComplete = onComplete;
    }

    public void Add(Func<PlayResult> func)
    {
        _chain.Enqueue(func);
    }

    public IEnumerable<PlayResult> Consume()
    {
        if (_current == null)
        {
            _current = _chain.Dequeue().Invoke();
            yield return _current;
        }

        if (!_current.Completed)
        {
            throw new Exception("Complete pending events before consuming further!");
        }

        if (_chain.Count == 0)
        {
            _onComplete?.Invoke();
            yield break;
        }

        _current = _chain.Dequeue().Invoke();
        yield return _current;
    }
}