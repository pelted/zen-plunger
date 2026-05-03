namespace ZenPlunger.Core.Tables;

public interface ITableCatalogImporter
{
    Task<TableCatalogImportResult> ImportAsync(string sourcePath, CancellationToken cancellationToken = default);
}

