namespace Kofi.Subscriptions.Domain.Subscriptions;

public enum DeliveryStatus
{
    Scheduled,
    // Due and announced; the order for it is on its way.
    Processing,
    Ready,
    Skipped,
    Cancelled,
}
