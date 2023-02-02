using System.Data;

namespace ScriptsOfTribute.Board;

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

    public static PlayerContext FromSerializedBoard(FullGameState fullGameState, SeededRandom rnd)
    {
        var p1 = Player.FromSerializedPlayer(fullGameState.CurrentPlayer, rnd, fullGameState.Cheats);
        var p2 = Player.FromSerializedPlayer(fullGameState.EnemyPlayer, rnd, fullGameState.Cheats);
        if (p1.ID == PlayerEnum.PLAYER1)
        {
            return new PlayerContext(p1, p2)
            {
                CurrentPlayerId = fullGameState.CurrentPlayer.PlayerID,
                EnemyPlayerId = fullGameState.EnemyPlayer.PlayerID,
            };   
        }
        else
        {
            return new PlayerContext(p2, p1)
            {
                CurrentPlayerId = fullGameState.CurrentPlayer.PlayerID,
                EnemyPlayerId = fullGameState.EnemyPlayer.PlayerID,
            };
        }
    }
}
