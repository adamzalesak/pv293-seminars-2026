using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Subscriptions.PrepareDelivery;

public record PrepareDeliveryCommand : ICommand
{
    public Guid SubscriptionId { get; init; }
    public Guid DeliveryId { get; init; }
}
