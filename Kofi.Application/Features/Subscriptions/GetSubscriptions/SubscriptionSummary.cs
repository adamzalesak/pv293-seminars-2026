namespace Kofi.Application.Features.Subscriptions.GetSubscriptions;

public record SubscriptionSummary
{
    public required Guid Id { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public required string ProductName { get; init; }
    public required string Status { get; init; }
    public required DateTime? NextDeliveryAt { get; init; }
}
