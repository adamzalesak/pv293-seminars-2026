namespace Kofi.BusinessLayer.Features.Orders.GetOrderDetail;

public record OrderDetailItem
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required string ProductName { get; init; }
    public required decimal UnitPrice { get; init; }
    public required int Quantity { get; init; }
}
