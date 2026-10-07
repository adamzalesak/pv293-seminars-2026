namespace Kofi.Catalog.Application.Features.Products.ChangeProductPrice;

public record ChangeProductPriceRequest
{
    public decimal Price { get; init; }
}
