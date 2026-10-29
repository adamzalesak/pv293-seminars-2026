namespace Kofi.Subscriptions.Contracts;

// The subscription as it stood when the event left, with its version. Events
// can arrive out of order; a subscriber that keeps a copy keeps the newest
// version and ignores an older one, whichever event carried it.
public record SubscriptionState(Guid ProductId, bool IsRunning, int Version);
