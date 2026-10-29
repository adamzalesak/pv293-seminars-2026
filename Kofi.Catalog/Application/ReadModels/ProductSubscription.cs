namespace Kofi.Catalog.Application.ReadModels;

// Catalog's own copy of what Subscriptions publishes: which product each
// running subscription is for. One row per subscription, so the same fact
// arriving twice does not count twice.
public class ProductSubscription
{
    public Guid SubscriptionId { get; set; }
    public Guid ProductId { get; set; }
    public bool IsActive { get; set; }

    // The version of the subscription this row was copied from.
    public int Version { get; set; }
}
