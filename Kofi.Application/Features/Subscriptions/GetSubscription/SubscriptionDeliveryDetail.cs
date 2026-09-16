namespace Kofi.Application.Features.Subscriptions.GetSubscription;

public record SubscriptionDeliveryDetail
{
    public required List<SubscriptionExtraDetail> Extras { get; init; }
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required string Currency { get; init; }
    public required DateTime ScheduledFor { get; init; }

    public required Guid? OrderId { get; init; }

    // The status of the order the delivery became, once it has one.
    public required string? OrderStatus { get; init; }
    public required string Status { get; init; }
}
