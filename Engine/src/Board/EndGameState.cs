namespace ScriptsOfTribute.Board;

public class EndGameState
{
    public readonly PlayerEnum Winner;
    public readonly GameEndReason Reason;
    public string AdditionalContext;

    public EndGameState(PlayerEnum winner, GameEndReason reason, string additionalContext = "")
    {
        Winner = winner;
        Reason = reason;
        AdditionalContext = additionalContext;
    }

    public override string ToString()
    {
        return $"Winner: {Winner.ToString()}, reason: {Reason.ToString()}{(AdditionalContext == "" ? "" : $"\n{AdditionalContext}")}";
    }

    public string ToSimpleString()
    {
        return $"{Winner} {Reason} {AdditionalContext}";
    }
}
