using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Products.GetProduct;

public class GetProductHandler(IProductRepository products)
    : IQueryHandler<GetProductQuery, ProductDetail?>
{
    public async Task<ProductDetail?> Handle(GetProductQuery query, CancellationToken ct)
    {
        var product = await products.GetById(query.Id);
        if (product == null)
        {
            return null;
        }

        return new ProductDetail
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            RoastedOn = product.RoastedOn,
            Origin = product.Origin,
            ImageUrl = product.ImageUrl,
        };
    }
}
