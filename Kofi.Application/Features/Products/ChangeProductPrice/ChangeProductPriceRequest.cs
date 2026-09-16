namespace Kofi.Application.Features.Products.ChangeProductPrice;

public record ChangeProductPriceRequest
{
    public decimal Price { get; init; }
}
