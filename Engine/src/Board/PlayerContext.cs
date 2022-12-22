using System.Data;

namespace TalesOfTribute.Board;

public interface IReadOnlyPlayerContext
{
    public PlayerEnum CurrentPlayerId { get; }
    public PlayerEnum EnemyPlayerId { get; }
    public Player CurrentPlayer { get; }
    public Player EnemyPlayer { get; }
}

public class PlayerContext : IReadOnlyPlayerContext
{
    private readonly Player[] _players = new Player[2];
    public PlayerEnum CurrentPlayerId { get; private set; } = PlayerEnum.PLAYER1;
    public PlayerEnum EnemyPlayerId { get; private set; } = PlayerEnum.PLAYER2;

    public Player CurrentPlayer => _players[(int)CurrentPlayerId];
    public Player EnemyPlayer => _players[(int)EnemyPlayerId];

    public PlayerContext(Player p1, Player p2)
    {
        _players[0] = p1;
        _players[1] = p2;
    }

    public void Swap()
    {
        (CurrentPlayerId, EnemyPlayerId) = (EnemyPlayerId, CurrentPlayerId);
    }
}
