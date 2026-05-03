namespace ZenPlunger.Core.Tables;

public interface ITableCatalogStore : ITableCatalog
{
    Task<TableCatalogDocument> LoadDocumentAsync(CancellationToken cancellationToken = default);

    Task SaveDocumentAsync(TableCatalogDocument document, CancellationToken cancellationToken = default);
}

