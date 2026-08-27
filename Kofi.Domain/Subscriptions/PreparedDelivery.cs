namespace Kofi.Domain.Subscriptions;

// A delivery that has just been prepared, as the order placed for it needs to
// see it. The subscription composes it; whoever places the order only copies it.
public record PreparedDelivery(Guid OrderId, IReadOnlyList<DeliveryLine> Lines);
