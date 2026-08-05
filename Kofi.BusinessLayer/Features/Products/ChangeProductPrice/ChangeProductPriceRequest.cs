namespace Kofi.BusinessLayer.Features.Products.ChangeProductPrice;

public record ChangeProductPriceRequest
{
    public decimal Price { get; init; }
}
