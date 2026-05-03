using ZenPlunger.Core.Tables;

namespace ZenPlunger.Core.Launching;

public sealed record LaunchRequest(
    PinballTable Table,
    string? GameMode = null);

