using ScriptsOfTribute.Board;
using ScriptsOfTribute.Serializers;

namespace ScriptsOfTribute.AI;

public abstract class AI
{
    public PlayerEnum Id { get; set; } = PlayerEnum.NO_PLAYER_SELECTED;

    // Round - which selection this is (first or second)
    public abstract PatronId SelectPatron(List<PatronId> availablePatrons, int round); // Will be called only twice
    public abstract Move Play(GameState gameState, List<Move> possibleMoves);
    public abstract void GameEnd(EndGameState state, FullGameState? finalBoardState);

    public List<(DateTime, string)> LogMessages { get; set; } = new();

    protected void Log(string message)
    {
        LogMessages.Add((DateTime.Now, message));
    }
}
