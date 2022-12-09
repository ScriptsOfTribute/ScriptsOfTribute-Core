namespace TalesOfTribute.Board;

public class EndGameState
{
    public readonly PlayerEnum Winner;
    public readonly GameEndReason Reason;
    public readonly string AdditionalContext;

    public EndGameState(PlayerEnum winner, GameEndReason reason, string additionalContext = "")
    {
        Winner = winner;
        Reason = reason;
        AdditionalContext = additionalContext;
    }
}
