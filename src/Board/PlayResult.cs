namespace TalesOfTribute;

public class PlayResult
{
    public bool Completed { get; protected set; } = true;
}

public class Success : PlayResult
{
}

public class BaseChoice : PlayResult
{
    public delegate void SuccessCallback();
    protected SuccessCallback? _successCallback;

    public BaseChoice()
    {
        Completed = false;
    }

    public void AddSuccessCallback(SuccessCallback successCallback)
    {
        _successCallback = successCallback;
    }
}

public class Choice<T> : BaseChoice
{
    public List<T> Choices { get; }

    public delegate PlayResult ChoiceCallback(T t);

    private ChoiceCallback _callback;

    public Choice(List<T> choices, ChoiceCallback callback) : base()
    {
        // Make sure choice of incorrect type is not created by mistake.
        if (typeof(T) != typeof(CardId) && typeof(T) != typeof(EffectType))
        {
            throw new Exception("Choice can only be made for cards or effects!");
        }
        
        Choices = choices;
        _callback = callback;
    }

    public PlayResult Commit(T t)
    {
        if (!Choices.Contains(t))
        {
            return new Failure("Invalid choice specified!");
        }
        var result = _callback(t);

        switch (result)
        {
            case Success:
                Completed = true;
                _successCallback?.Invoke();
                break;
            case BaseChoice choice:
                choice.AddSuccessCallback(() => Completed = true);
                break;
        }

        return result;
    }
}

// Failure should only be returned on user input error.
// If error is not a consequence of a decision, an exception should be thrown.
public class Failure : PlayResult
{
    public string Reason { get; }

    public Failure(string reason)
    {
        Reason = reason;
    }
}

