using ZenPlunger.Core.Tables;

namespace ZenPlunger.Core.Launching;

public sealed record LaunchRequest
{
    public const string DefaultGameMode = "Classic";

    public LaunchRequest(PinballTable table, string? gameMode = DefaultGameMode)
    {
        ArgumentNullException.ThrowIfNull(table);

        Table = table;
        GameMode = gameMode;
    }

    public PinballTable Table { get; init; }

    public string? GameMode { get; init; }
}
