using Kofi.DataAccessLayer.Data;
using Kofi.BusinessLayer.Dtos;
using Kofi.BusinessLayer.Mappers;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Services;

public class ProductService(
    AppDbContext db,
    ProductRepository products,
    AuditService audit)
{
    public async Task<PagedResult<ProductDto>> GetProducts(string? search, string? sort, int page, int pageSize)
    {
        var result = await products.Search(search, sort, page, pageSize);
        return new PagedResult<ProductDto>
        {
            Items = result.Items.Select(p => p.ToDto()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
        };
    }

    public async Task<ProductDto?> GetProduct(Guid id)
    {
        var product = await products.GetById(id);
        return product?.ToDto();
    }

    public async Task<ProductDto> CreateProduct(ProductDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new Exception("Name is required");
        }
        if (request.Price <= 0)
        {
            throw new Exception("Price must be positive");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true,
            RoastedOn = request.RoastedOn,
            Origin = request.Origin,
            ImageUrl = request.ImageUrl,
        };

        using var transaction = await db.Database.BeginTransactionAsync();

        await products.Add(product);

        await transaction.CommitAsync();

        audit.Record("ProductCreated", product.Id);

        return product.ToDto();
    }

    public async Task ChangePrice(Guid id, decimal newPrice)
    {
        var product = await products.GetById(id);
        if (product == null)
        {
            throw new Exception("Product not found");
        }

        using var transaction = await db.Database.BeginTransactionAsync();

        product.Price = newPrice;
        await products.Update(product);

        await transaction.CommitAsync();
    }

    public async Task Deactivate(Guid id)
    {
        var product = await products.GetById(id);
        if (product == null)
        {
            throw new Exception("Product not found");
        }

        product.IsActive = false;
        await products.Update(product);
    }

    public async Task Restock(Guid id, int quantity)
    {
        var product = await products.GetById(id);
        if (product == null)
        {
            throw new Exception("Product not found");
        }

        product.StockQuantity = product.StockQuantity + quantity;
        await products.Update(product);
    }
}
