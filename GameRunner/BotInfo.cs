using ScriptsOfTribute.AI;

namespace GameRunner;

internal abstract class BotInfo
{
    public Type BotType { get; set; }
    public string? BotName { get; set; }
    public string? BotFullName => BotType?.FullName;
    public abstract AI CreateBotInstance();
}

internal class LocalBotInfo : BotInfo
{
    public override AI CreateBotInstance()
    {
        return (AI)Activator.CreateInstance(BotType)!;
    }
}

internal class ExternalBotInfo : BotInfo
{
    public string? ProgramName { get; set; }
    public string? FileName { get; set; }

    public override AI CreateBotInstance()
    {
        return (AI)Activator.CreateInstance(BotType, ProgramName, FileName)!;
    }
}

internal class gRPCBotInfo : BotInfo
{
    public string? HostName { get; set; }
    public int? ClientPort { get; set; }
    public int? ServerPort { get; set; }

    public override AI CreateBotInstance()
    {
        return (AI)Activator.CreateInstance(BotType, HostName, ClientPort, ServerPort)!;
    }
}


