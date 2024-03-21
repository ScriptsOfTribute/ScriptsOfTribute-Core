using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute.AI;

public abstract class AI
{
    public PlayerEnum Id { get; set; } = PlayerEnum.NO_PLAYER_SELECTED;

    /// <summary>
    /// Optional method that lets bot prepare itself before the game
    /// To make it fair there's a timelimit of 2 seconds for this function
    /// </summary>
    public virtual void PregamePrepare(){}
    // Round - which selection this is (first or second)
    public abstract PatronId SelectPatron(List<PatronId> availablePatrons, int round); // Will be called only twice
    public abstract Move Play(GameState gameState, List<Move> possibleMoves, TimeSpan remainingTime);
    public abstract void GameEnd(EndGameState state, FullGameState? finalBoardState);

    public List<(DateTime, string)> LogMessages { get; set; } = new();

    protected void Log(string message)
    {
        LogMessages.Add((DateTime.Now, message));
    }
}
