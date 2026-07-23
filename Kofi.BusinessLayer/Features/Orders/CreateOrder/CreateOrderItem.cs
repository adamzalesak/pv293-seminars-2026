namespace Kofi.BusinessLayer.Features.Orders.CreateOrder;

public record CreateOrderItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
