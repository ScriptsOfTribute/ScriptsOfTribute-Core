namespace TalesOfTribute;

public enum ResultType
{
    SUCCESS,
    FAILURE,
    CHOICE
}

public class Result
{
}

public class Success : Result
{
}

public class Choice<T> : Result
{
    public List<T> Choices { get; }
    public delegate Result ChoiceCallback(T t);
    public delegate void NotifyCallback();

    private ChoiceCallback _callback;
    private NotifyCallback _resolvedCallback;

    public Choice(List<T> choices, ChoiceCallback callback)
    {
        // Make sure choice of incorrect type is not created by mistake.
        if (typeof(T) != typeof(CardId) && typeof(T) != typeof(EffectType))
        {
            throw new Exception("Choice can only be made for cards or effects!");
        }
        
        Choices = choices;
        _callback = callback;
    }

    public void AddCallback(ChoiceCallback callback)
    {
        _callback += callback;
    }
    
    public void AddResolvedCallback(NotifyCallback callback)
    {
        _resolvedCallback += callback;
    }

    public Result Commit(T t)
    {
        if (!Choices.Contains(t))
        {
            return new Failure("Invalid card specified!");
        }
        var result = _callback(t);
        if (result is Success)
        {
            _resolvedCallback();
        }
        return result;
    }
}

public class Failure : Result
{
    public string Reason { get; }

    public Failure(string reason)
    {
        Reason = reason;
    }
}

