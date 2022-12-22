using TalesOfTribute.Board.CardAction;

namespace TalesOfTribute;

public interface ISimpleResult
{
}

public abstract class PlayResult
{
    public bool Completed { get; protected set; } = true;
}

public class Success : PlayResult, ISimpleResult
{
}

public class BaseChoice : PlayResult
{
    public delegate void SuccessCallback();
    public delegate void ChoiceFinishCallback(PlayResult result);
    protected SuccessCallback? _successCallback;
    protected ChoiceFinishCallback? _choiceFinishCallback;

    public BaseChoice()
    {
        Completed = false;
    }

    public void AddSuccessCallback(SuccessCallback successCallback)
    {
        _successCallback = successCallback;
    }
    
    public void AddChoiceFinishCallback(ChoiceFinishCallback choiceFinishCallback)
    {
        _choiceFinishCallback = choiceFinishCallback;
    }
}

public interface IReadOnlyChoice<T>
{
    List<T> PossibleChoices { get; }
    int MaxChoiceAmount { get; }
    int MinChoiceAmount { get; }
    public ChoiceContext? Context { get; }
}

public class Choice<T> : BaseChoice, IReadOnlyChoice<T>
{
    public List<T> PossibleChoices { get; }
    public int MaxChoiceAmount { get; } = 1;
    public int MinChoiceAmount { get; } = 0;

    public delegate PlayResult ChoiceCallback(List<T> t, ComplexEffectExecutor executor);

    private readonly ChoiceCallback _callback;
    public ChoiceContext? Context { get; }

    public Choice(List<T> possibleChoices, ChoiceCallback callback, ChoiceContext? context) : base()
    {
        // Make sure choice of incorrect type is not created by mistake.
        if (typeof(T) != typeof(EffectType) && typeof(T) != typeof(Card))
        {
            throw new Exception("Choice can only be made for cards or effects!");
        }

        PossibleChoices = possibleChoices;
        _callback = callback;
        Context = context;
    }

    public Choice(List<T> possibleChoices, ChoiceCallback callback, ChoiceContext? context, int maxChoiceAmount, int minChoiceAmount = 0) : this(possibleChoices, callback, context)
    {
        if (minChoiceAmount > possibleChoices.Count)
        {
            throw new Exception("Invalid choice amount specified!");
        }

        MaxChoiceAmount = maxChoiceAmount;
        MinChoiceAmount = minChoiceAmount;
    }

    public PlayResult Choose(List<T> choices, ComplexEffectExecutor executor)
    {
        if (PossibleChoices.Count == 0)
        {
            var dummyResult = new Success();
            HandleResult(dummyResult);
            return dummyResult;
        }
        // Check if all choices are in possible choices.
        if (choices.Except(PossibleChoices).Any() || choices.Count > MaxChoiceAmount || choices.Count < MinChoiceAmount)
        {
            return new Failure("Invalid choices specified!");
        }
        var result = _callback(choices, executor);

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
        _choiceFinishCallback?.Invoke(result);
    }

    private void OnSuccess()
    {
        Completed = true;
        _successCallback?.Invoke();
    }
}

// Failure should only be returned on user input error.
// If error is not a consequence of a decision, an exception should be thrown.
public class Failure : PlayResult, ISimpleResult
{
    public string Reason { get; }

    public Failure(string reason)
    {
        Reason = reason;
    }
}
