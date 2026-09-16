using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Subscriptions.RemoveDeliveryExtra;

public record RemoveDeliveryExtraCommand : ICommand, IForSubscription
{
    public Guid SubscriptionId { get; init; }
    public Guid DeliveryId { get; init; }
    public Guid ProductId { get; init; }
}
