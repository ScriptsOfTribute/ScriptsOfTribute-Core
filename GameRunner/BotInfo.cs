using ScriptsOfTribute.AI;

namespace GameRunner;

internal class BotInfo
{
    public Type? BotType { get; set; }
    public string? BotName => BotType?.Name;
    public string? BotFullName => BotType?.FullName;
    public bool IsExternal => BotType == typeof(ExternalAIAdapter);
    public string? ProgramName { get; set; }
    public string? FileName { get; set; }

}
