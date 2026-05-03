namespace ZenPlunger.Core.Tables;

public sealed record TableCatalogImportResult(
    int ImportedCount,
    IReadOnlyList<string> Warnings)
{
    public bool HasWarnings => Warnings.Count > 0;
}

