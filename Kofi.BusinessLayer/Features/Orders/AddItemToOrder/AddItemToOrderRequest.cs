namespace Kofi.BusinessLayer.Features.Orders.AddItemToOrder;

public record AddItemToOrderRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
