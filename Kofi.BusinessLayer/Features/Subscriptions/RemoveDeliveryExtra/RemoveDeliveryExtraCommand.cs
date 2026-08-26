using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Subscriptions.RemoveDeliveryExtra;

public record RemoveDeliveryExtraCommand : ICommand, IForSubscription
{
    public Guid SubscriptionId { get; init; }
    public Guid DeliveryId { get; init; }
    public Guid ProductId { get; init; }
}
