using System.Text;

namespace TalesOfTribute.utils;

public class Logger
{
    private TextWriter _writer;

    public Logger(TextWriter writer)
    {
        _writer = writer;
    }

    public void Log(PlayerEnum player, string message)
    {
        _writer.Write($"[{player}] {message}\n");
    }

    public void Log(PlayerEnum player, DateTime timestamp, int turn, int move, string message)
    {
        var s = $"[{player}][{timestamp:hh:mm:ss:fff}][{turn:000}][{move:00}] {message}\n";
        _writer.Write(s);
    }

    public void Log(List<(DateTime, string)> messagesWithTimestamp, PlayerEnum player, int turn, int move)
    {
        messagesWithTimestamp.ForEach(m => Log(player, m.Item1, turn, move, m.Item2));
    }

    public void Flush()
    {
        _writer.Flush();
    }
}
