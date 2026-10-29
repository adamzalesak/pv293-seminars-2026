using Kofi.Subscriptions.Domain.Subscriptions;

namespace Kofi.Subscriptions.Application.Services;

public interface IDeliveryProducts
{
    // Part of the decision to prepare a delivery, so it is checked in the
    // command: every product in it must still be on sale.
    Task EnsureOnSale(Delivery delivery, CancellationToken ct);
}
