namespace Kofi.BusinessLayer.Features.Subscriptions.GetSubscription;

public record SubscriptionExtraDetail
{
    public required Guid ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required string Currency { get; init; }
}
