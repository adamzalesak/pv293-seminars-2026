using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Subscriptions.PrepareDelivery;

public record PrepareDeliveryCommand : ICommand
{
    public Guid SubscriptionId { get; init; }
    public Guid DeliveryId { get; init; }

}
