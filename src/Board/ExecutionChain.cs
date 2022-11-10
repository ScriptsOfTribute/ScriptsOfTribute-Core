namespace TalesOfTribute;

public class ExecutionChain
{
    public delegate void OnComplete();

    private List<Func<PlayResult>> _chain;
    private PlayResult? _current;
    private OnComplete _onComplete;

    public ExecutionChain(List<Func<PlayResult>> chain)
    {
        this._chain = chain;
    }

    public void AddCompleteCallback(OnComplete onComplete)
    {
        _onComplete = onComplete;
    }

    public IEnumerable<PlayResult> Consume()
    {
        if (_current == null)
        {
            _current = _chain[0]();
            _chain.RemoveAt(0);
            yield return _current;
        }

        if (!_current.Completed)
        {
            throw new Exception("Complete pending events before consuming further!");
        }

        if (_chain.Count == 0)
        {
            _onComplete();
            yield break;
        }
        
        _current = _chain[0]();
        _chain.RemoveAt(0);
        yield return _current;
    }
}