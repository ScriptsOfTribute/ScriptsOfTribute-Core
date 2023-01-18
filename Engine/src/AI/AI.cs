using TalesOfTribute.Board;
using TalesOfTribute.Serializers;

namespace TalesOfTribute.AI;

public abstract class AI
{
    private ulong _seed;
    public PlayerEnum Id { get; set; } = PlayerEnum.NO_PLAYER_SELECTED;

    public ulong Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            Rng = new(value);
        }
    }

    public SeededRandom Rng { get; private set; } = new();

    // Round - which selection this is (first or second)
    public abstract PatronId SelectPatron(List<PatronId> availablePatrons, int round); // Will be called only twice
    public abstract Move Play(GameState gameState, List<Move> possibleMoves);
    public abstract void GameEnd(EndGameState state);

    public List<(DateTime, string)> LogMessages { get; set; } = new();

    protected void Log(string message)
    {
        LogMessages.Add((DateTime.Now, message));
    }
}
