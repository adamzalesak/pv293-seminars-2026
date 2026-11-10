using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Subscriptions.AddDeliveryExtra;

public record AddDeliveryExtraCommand : ICommand, IForSubscription
{
    public Guid SubscriptionId { get; init; }
    public Guid DeliveryId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
