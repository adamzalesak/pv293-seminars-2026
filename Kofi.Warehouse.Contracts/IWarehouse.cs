namespace Kofi.Warehouse.Contracts;

public interface IWarehouse
{
    Task AddProduct(Guid productId, int quantity, string shelfLocation = "A-01", CancellationToken ct = default);
    Task CheckAvailable(IReadOnlyList<StockLine> lines, CancellationToken ct = default);
    Task Reserve(IReadOnlyList<StockLine> lines, CancellationToken ct = default);
    Task Release(Guid productId, int quantity, CancellationToken ct = default);
    Task<int> GetAvailable(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, int>> GetAvailable(IReadOnlyList<Guid> ids, CancellationToken ct = default);
}
