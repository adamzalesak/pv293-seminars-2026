using Kofi.BusinessLayer.Dtos;
using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            ImageUrl = product.ImageUrl,
            RoastedOn = product.RoastedOn,
            Origin = product.Origin,
        };
    }
}
