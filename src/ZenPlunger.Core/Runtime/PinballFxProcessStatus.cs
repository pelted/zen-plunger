namespace ZenPlunger.Core.Runtime;

public sealed record PinballFxProcessStatus(
    bool IsRunning,
    int ProcessCount,
    IReadOnlyList<string> MatchedProcessNames);
