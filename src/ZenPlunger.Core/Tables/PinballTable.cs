namespace ZenPlunger.Core.Tables;

public sealed record PinballTable(
    string Id,
    string DisplayName,
    string? Collection = null)
{
    public override string ToString() => DisplayName;
}

