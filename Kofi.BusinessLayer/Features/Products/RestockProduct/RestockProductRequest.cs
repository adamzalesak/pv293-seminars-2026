namespace Kofi.BusinessLayer.Features.Products.RestockProduct;

public record RestockProductRequest
{
    public int Quantity { get; init; }
}
