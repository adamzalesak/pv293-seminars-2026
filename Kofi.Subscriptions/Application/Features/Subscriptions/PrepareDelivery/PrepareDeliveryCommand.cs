using Kofi.Common.Abstractions;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.PrepareDelivery;

public record PrepareDeliveryCommand : ICommand
{
    public Guid SubscriptionId { get; init; }
    public Guid DeliveryId { get; init; }
}
