using Wolverine.Attributes;

namespace Kofi.Ordering.Application.Payments;

// The provider's webhook, taken in. EventId is the provider's own id for the
// notification, so a webhook it sends again is recognised as the same one.
public record PaymentResultReceived(
    [property: DeduplicationIdentity] Guid EventId,
    Guid PaymentId,
    Guid OrderId,
    string Status);
