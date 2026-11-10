namespace Kofi.Application.Features.Products.RestockProduct;

public record RestockProductRequest
{
    public int Quantity { get; init; }
}
