namespace ZenPlunger.Core.Tables;

public sealed record TableCatalogDocument(
    int SchemaVersion,
    IReadOnlyList<PinballTable> Tables)
{
    public const int CurrentSchemaVersion = 1;

    public static TableCatalogDocument Empty { get; } = new(CurrentSchemaVersion, []);
}

