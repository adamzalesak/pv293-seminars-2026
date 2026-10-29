using Kofi.Contracts;

namespace Kofi.Ordering.Contracts;

// No order could be placed for a subscription delivery.
public record OrderFailedIntegrationEvent(Guid DeliveryId, string Reason) : IIntegrationEvent;
