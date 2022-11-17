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
    public List<T> PossibleChoices { get; }
    public int MaxChoiceAmount { get; } = 1;
    public int MinChoiceAmount { get; } = 0;

    public delegate PlayResult ChoiceCallback(List<T> t);

    private readonly ChoiceCallback _callback;

    public Choice(List<T> possibleChoices, ChoiceCallback callback) : base()
    {
        // Make sure choice of incorrect type is not created by mistake.
        if (typeof(T) != typeof(CardId) && typeof(T) != typeof(EffectType) && typeof(T) != typeof(Card))
        {
            throw new Exception("Choice can only be made for cards or effects!");
        }

        PossibleChoices = possibleChoices;
        _callback = callback;
    }

    public Choice(List<T> possibleChoices, ChoiceCallback callback, int maxChoiceAmount, int minChoiceAmount = 0) : this(possibleChoices, callback)
    {
        if (maxChoiceAmount > possibleChoices.Count)
        {
            throw new Exception("Invalid choice amount specified!");
        }

        MaxChoiceAmount = maxChoiceAmount;
        MinChoiceAmount = minChoiceAmount;
    }

    public PlayResult Choose(T t)
    {
        if (!PossibleChoices.Contains(t) || MinChoiceAmount > 1)
        {
            return new Failure("Invalid choice specified!");
        }
        var result = _callback(new List<T> { t });

        HandleResult(result);

        return result;
    }

    public PlayResult Choose(List<T> choices)
    {
        // Check if all choices are in possible choices.
        if (choices.Except(PossibleChoices).Any() || choices.Count > MaxChoiceAmount || choices.Count < MinChoiceAmount)
        {
            return new Failure("Invalid choices specified!");
        }
        var result = _callback(choices);

        HandleResult(result);

        return result;
    }

    private void HandleResult(PlayResult result)
    {
        switch (result)
        {
            case Success:
                OnSuccess();
                break;
            case BaseChoice choice:
                choice.AddSuccessCallback(OnSuccess);
                break;
        }
    }

    private void OnSuccess()
    {
        Completed = true;
        _successCallback?.Invoke();
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
