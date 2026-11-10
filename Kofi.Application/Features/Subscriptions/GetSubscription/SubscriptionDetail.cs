namespace Kofi.Application.Features.Subscriptions.GetSubscription;

public record SubscriptionDetail
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required Guid ProductId { get; init; }
    public required string ProductName { get; init; }
    public required string Status { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required string Currency { get; init; }
    public required int IntervalDays { get; init; }

    public required DateTime CreatedAt { get; init; }
    public required DateTime? ActivatedAt { get; init; }
    public required DateTime? PausedAt { get; init; }
    public required DateTime? CancelledAt { get; init; }
    public required DateTime? NextDeliveryAt { get; init; }
    public required List<SubscriptionDeliveryDetail> Deliveries { get; init; }
}
