namespace ZenPlunger.Core.Tables;

public interface ITableCatalog
{
    Task<IReadOnlyList<PinballTable>> GetTablesAsync(CancellationToken cancellationToken = default);
}

