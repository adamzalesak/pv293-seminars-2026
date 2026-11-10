namespace Kofi.Ordering.Application.Ports;

public interface IProductCatalog
{
    Task<ProductOffer?> GetProduct(Guid id, CancellationToken ct = default);
}
