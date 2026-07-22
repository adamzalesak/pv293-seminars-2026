using Kofi.BusinessLayer.Dtos;

namespace Kofi.BusinessLayer.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProducts(
        string? search, string? sort, int page, int pageSize, bool includeInactive = false);
    Task<ProductDto?> GetProduct(Guid id);
    Task<ProductDto> CreateProduct(ProductDto request);
    Task UpdateProduct(Guid id, ProductDto request);
    Task ChangePrice(Guid id, decimal newPrice);
    Task Deactivate(Guid id);
    Task Activate(Guid id);
    Task Restock(Guid id, int quantity);
}
