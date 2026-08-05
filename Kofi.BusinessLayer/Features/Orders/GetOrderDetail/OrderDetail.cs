namespace Kofi.BusinessLayer.Features.Orders.GetOrderDetail;

public record OrderDetail
{
    public required Guid Id { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required string Street { get; init; }
    public required string City { get; init; }
    public required string Zip { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required decimal ItemsTotal { get; init; }
    public required decimal Shipping { get; init; }
    public required decimal Total { get; init; }
    public required IReadOnlyList<OrderDetailItem> Items { get; init; }
}
