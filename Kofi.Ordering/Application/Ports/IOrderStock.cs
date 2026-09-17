namespace Kofi.Ordering.Application.Ports;

public interface IOrderStock
{
    Task CheckAvailable(IReadOnlyList<StockLine> lines, CancellationToken ct = default);
    Task Reserve(IReadOnlyList<StockLine> lines, CancellationToken ct = default);
    Task Release(Guid productId, int quantity, CancellationToken ct = default);
}
