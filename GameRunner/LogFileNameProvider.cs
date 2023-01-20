namespace GameRunner;

public class LogFileNameProvider
{
    private readonly DirectoryInfo _directory;

    public LogFileNameProvider(DirectoryInfo directory)
    {
        _directory = directory;
    }
    
    public (TextWriter, TextWriter) GetForPlayers(ulong seed, bool p1Enabled, bool p2Enabled)
    {
        var p1Dest = p1Enabled ?  File.CreateText(Path.Combine(_directory.FullName, $"{seed}_p1.log")) : Console.Out;
        var p2Dest = p2Enabled ? File.CreateText(Path.Combine(_directory.FullName, $"{seed}_p2.log")) : Console.Out;
        return (p1Dest, p2Dest);
    }
}
