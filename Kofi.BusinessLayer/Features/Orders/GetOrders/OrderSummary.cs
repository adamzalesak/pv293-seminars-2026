namespace Kofi.BusinessLayer.Features.Orders.GetOrders;

public record OrderSummary
{
    public required Guid Id { get; init; }
    public required string CustomerName { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required decimal Total { get; init; }
}
