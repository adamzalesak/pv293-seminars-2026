namespace Kofi.Catalog.Contracts;

public interface ICatalog
{
    Task<ProductInfo?> GetProduct(Guid id, CancellationToken ct = default);
}
