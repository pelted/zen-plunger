namespace ZenPlunger.Core.Tables;

public sealed record PinballTable(
    string Id,
    string DisplayName,
    string? Collection = null,
    TableMetadata? Metadata = null,
    TableAssets? Assets = null,
    string? Notes = null)
{
    public override string ToString() => DisplayName;
}
