using System.Text;

namespace ScriptsOfTribute.utils;

public class Logger
{
    public TextWriter P1LogTarget { get; set; } = Console.Out;
    public TextWriter P2LogTarget { get; set; } = Console.Out;
    public bool P1LoggerEnabled { get; set; } = false;
    public bool P2LoggerEnabled { get; set; } = false;

    public Logger()
    {
    }

    public void Log(PlayerEnum player, string message)
    {
        if (!EnabledFor(player)) return;
        GetWriterFor(player)?.Write($"[{player}] {message}\n");
    }

    public void Log(PlayerEnum player, DateTime timestamp, int turn, int move, string message)
    {
        if (!EnabledFor(player)) return;
        var s = $"[{player}][{timestamp:hh:mm:ss:fff}][{turn:000}][{move:00}] {message}\n";
        GetWriterFor(player)?.Write(s);
    }

    public void Log(List<(DateTime, string)> messagesWithTimestamp, PlayerEnum player, int turn, int move)
    {
        if (!EnabledFor(player)) return;
        messagesWithTimestamp.ForEach(m => Log(player, m.Item1, turn, move, m.Item2));
    }

    public void Flush()
    {
        P1LogTarget.Flush();
        P2LogTarget.Flush();
    }

    private TextWriter? GetWriterFor(PlayerEnum player) =>
        player switch
        {
            PlayerEnum.PLAYER1 => P1LogTarget,
            PlayerEnum.PLAYER2 => P2LogTarget,
            _ => null,
        };
    public bool EnabledFor(PlayerEnum player) =>
        player switch
        {
            PlayerEnum.PLAYER1 => P1LoggerEnabled,
            PlayerEnum.PLAYER2 => P2LoggerEnabled,
            _ => false,
        };
}
