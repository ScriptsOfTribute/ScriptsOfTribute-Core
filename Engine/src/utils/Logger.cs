using System.Text;

namespace TalesOfTribute.utils;

public class Logger
{
    private TextWriter _writer;

    public Logger(TextWriter writer)
    {
        _writer = writer;
    }

    public void Log(string message)
    {
        _writer.Write(message + '\n');
    }
    
    public void Log(PlayerEnum player, DateTime timestamp, int turn, int move, string message)
    {
        var s = $"[{player}][{timestamp:hh:mm:ss:fff}][{turn:000}][{move:00}] {message}\n";
        _writer.Write(s);
    }
}
