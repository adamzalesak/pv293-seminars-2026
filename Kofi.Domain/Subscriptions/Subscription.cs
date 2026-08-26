using Kofi.Domain.Shared;

namespace Kofi.Domain.Subscriptions;

// The consistency boundary: the plan, its deliveries, and the rules that
// decide which change is allowed. Customer, product and order are outside it
// and are referred to by id.
//
// Every method here answers one question — is this change legal from where
// we are now — and then makes it. None of them saves, sends mail or reads
// the clock; the caller passes the time in, and facts from elsewhere with it.
public class Subscription : AggregateRoot
{
    private readonly List<Delivery> _deliveries = new();

    private Subscription()
    {
    }

    public Guid CustomerId { get; private set; }
    public Plan Plan { get; private set; } = null!;
    public SubscriptionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public DateTime? PausedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? NextDeliveryAt { get; private set; }

    public IReadOnlyList<Delivery> Deliveries => _deliveries.AsReadOnly();

    // The customer subscribes with a plan and the day of the first delivery.
    // Nothing is planned or charged until they authorise a card; until then
    // the chosen day is the only date the subscription has.
    public static Subscription Create(Guid customerId, Plan plan, DateTime firstDeliveryOn, DateTime now)
    {
        RejectUnless(firstDeliveryOn.Date >= now.Date,
            "The first delivery cannot be in the past.");

        return new()
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Plan = plan,
            Status = SubscriptionStatus.Incomplete,
            CreatedAt = now,
            NextDeliveryAt = firstDeliveryOn.Date,
        };
    }

    // The card is authorised: the first delivery is planned for the day the
    // customer chose, or for today if that day has already passed. A delivery
    // due today is prepared like any other due delivery, through PrepareDelivery.
    public void Activate(DateTime now)
    {
        RejectUnless(Status == SubscriptionStatus.Incomplete,
            "Only an incomplete subscription can be activated.");

        Status = SubscriptionStatus.Active;
        ActivatedAt = now;
        Schedule(NextDeliveryAt!.Value < now.Date ? now.Date : NextDeliveryAt.Value);

        Raise(new SubscriptionActivated(Id));
    }

    public void Pause(DateTime now)
    {
        // TODO: only an Active subscription can pause; the waiting delivery and
        // its extras stay; remember when it paused. Helper: RejectUnless.
        throw new NotImplementedException();
    }

    // The customer says when the next delivery should come; today is fine.
    public void Resume(DateTime nextDeliveryOn, DateTime now)
    {
        RejectUnless(Status == SubscriptionStatus.Paused,
            "Only a paused subscription can be resumed.");
        RejectUnless(nextDeliveryOn.Date >= now.Date,
            "The next delivery cannot be in the past.");

        Status = SubscriptionStatus.Active;
        PausedAt = null;

        // The delivery that was waiting keeps its place in the queue; it only
        // gets a new date.
        var next = nextDeliveryOn.Date;
        if (NextScheduled() is { } waiting)
        {
            waiting.Reschedule(next, Plan);
            NextDeliveryAt = next;
        }
        else
        {
            Schedule(next);
        }

    }

    public void Cancel(DateTime now)
    {
        RejectUnless(Status != SubscriptionStatus.Cancelled,
            "This subscription is already cancelled.");

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = now;
        NextDeliveryAt = null;

        // Only what was merely planned. A delivery an order was already
        // placed for runs its own course and can still arrive; stopping that
        // parcel means cancelling the order, not the subscription.
        foreach (var delivery in _deliveries.Where(d => d.IsScheduled).ToList())
        {
            delivery.Cancel();
        }

    }

    public void ChangePlan(Plan plan)
    {
        // TODO: an Active or Paused subscription changes its plan; only Scheduled
        // deliveries follow it and keep their dates and extras. Helpers:
        // RejectUnless, FollowPlan.
        throw new NotImplementedException();
    }

    public void SkipNextDelivery(DateTime now)
    {
        // TODO: only an Active subscription skips its next Scheduled delivery, even a
        // due one; it stays in history and the next one is planned an interval
        // later from the plan alone. Helpers: RejectUnless, NextScheduled, Schedule.
        throw new NotImplementedException();
    }

    public void AddExtra(Guid deliveryId, Guid productId, string productName, int quantity, Price unitPrice)
    {
        // TODO: only the subscription's own Scheduled delivery takes an extra, while
        // the subscription is Active or Paused. Helper: EditableDelivery.
        throw new NotImplementedException();
    }

    public void RemoveExtra(Guid deliveryId, Guid productId)
    {
        EditableDelivery(deliveryId).RemoveExtra(productId);
    }

    public void PrepareDelivery(Guid deliveryId, Guid orderId, DateTime now)
    {
        RejectUnless(Status == SubscriptionStatus.Active, "Only an active subscription can prepare a delivery.");
        var delivery = EditableDelivery(deliveryId);
        RejectUnless(delivery.ScheduledFor <= now, "This delivery is not due yet.");
        delivery.Prepare(orderId);
        Schedule(delivery.ScheduledFor.AddDays(Plan.IntervalDays));
    }

    private Delivery EditableDelivery(Guid deliveryId)
    {
        RejectUnless(Status is SubscriptionStatus.Active or SubscriptionStatus.Paused,
            "Only a running subscription can change a delivery.");
        var delivery = _deliveries.SingleOrDefault(d => d.Id == deliveryId)
            ?? throw new DomainRuleException("This subscription has no such delivery.");
        RejectUnless(delivery.IsScheduled, "Only a planned delivery can be changed.");
        return delivery;
    }

    private Delivery? NextScheduled() =>
        _deliveries.Where(d => d.IsScheduled).OrderBy(d => d.ScheduledFor).FirstOrDefault();

    private Delivery Schedule(DateTime scheduledFor)
    {
        var delivery = Delivery.Schedule(scheduledFor, Plan);
        _deliveries.Add(delivery);
        NextDeliveryAt = scheduledFor;
        return delivery;
    }

    private static void RejectUnless(bool allowed, string because)
    {
        if (!allowed)
        {
            throw new DomainRuleException(because);
        }
    }
}
