using Kofi.Domain.Shared;

namespace Kofi.Domain.Subscriptions;

// Owned by Subscription. Nothing outside the aggregate creates or changes one,
// which is why every method here is internal. The price is the one agreed when
// the delivery was planned; a later plan does not reach back into it.
public class Delivery
{
    private readonly List<DeliveryExtra> _extras = new();

    private Delivery()
    {
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Price UnitPrice { get; private set; } = null!;
    public DateTime ScheduledFor { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public Guid? OrderId { get; private set; }

    public IReadOnlyList<DeliveryExtra> Extras => _extras.AsReadOnly();

    internal static Delivery Schedule(DateTime scheduledFor, Plan plan) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = plan.ProductId,
        Quantity = plan.Quantity,
        UnitPrice = Snapshot(plan.UnitPrice),
        ScheduledFor = scheduledFor,
        Status = DeliveryStatus.Scheduled,
    };

    internal bool IsScheduled => Status == DeliveryStatus.Scheduled;


    // An order has been placed for this delivery; from now on it is being
    // fulfilled and the plan no longer moves it.
    internal void Prepare(Guid orderId)
    {
        RejectUnless(IsScheduled, "Only a scheduled delivery can be prepared.");
        Status = DeliveryStatus.Ready;
        OrderId = orderId;
    }

    internal void Skip()
    {
        RejectUnless(IsScheduled, "Only a scheduled delivery can be skipped.");
        Status = DeliveryStatus.Skipped;
    }

    internal void Cancel()
    {
        RejectUnless(IsScheduled, "Only a scheduled delivery can be cancelled.");
        Status = DeliveryStatus.Cancelled;
    }

    internal void Reschedule(DateTime scheduledFor, Plan plan)
    {
        RejectUnless(IsScheduled, "Only a scheduled delivery can be moved.");
        ScheduledFor = scheduledFor;
        FollowPlan(plan);
    }

    internal void FollowPlan(Plan plan)
    {
        RejectUnless(IsScheduled, "Only a scheduled delivery can change its plan.");
        ProductId = plan.ProductId;
        Quantity = plan.Quantity;
        UnitPrice = Snapshot(plan.UnitPrice);
    }

    internal void AddExtra(Guid productId, string productName, int quantity, Price unitPrice)
    {
        RejectUnless(quantity is > 0 and <= 100, "An extra needs between one and one hundred units.");
        RejectUnless(unitPrice.Amount > 0 && unitPrice.Currency == UnitPrice.Currency,
            "An extra needs a positive price in the delivery currency.");
        var existing = _extras.SingleOrDefault(e => e.ProductId == productId);
        RejectUnless((existing?.Quantity ?? 0) + quantity <= 100, "An extra cannot exceed one hundred units.");
        if (existing == null) _extras.Add(new DeliveryExtra(productId, productName, quantity, unitPrice));
        else existing.Increase(quantity);
    }

    internal void RemoveExtra(Guid productId)
    {
        var extra = _extras.SingleOrDefault(e => e.ProductId == productId)
            ?? throw new DomainRuleException("This delivery has no such extra.");
        _extras.Remove(extra);
    }

    // The price of this delivery is its own, even when it matches the plan:
    // the mapping gives every delivery its own two columns to write into.
    private static Price Snapshot(Price unitPrice) => new(unitPrice.Amount, unitPrice.Currency);

    private static void RejectUnless(bool allowed, string because)
    {
        if (!allowed)
        {
            throw new DomainRuleException(because);
        }
    }
}
