namespace Kofi.BusinessLayer.Features.Orders.CreateOrder;

public record CreateOrderResponse
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
    public required decimal Total { get; init; }
}
