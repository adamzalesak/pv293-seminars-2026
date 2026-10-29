namespace Kofi.Subscriptions.Domain.Subscriptions;

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
    public static Subscription Create(Guid customerId, OfferedProduct product, Quantity quantity, int intervalDays,
        DateTime firstDeliveryOn, DateTime now)
    {
        product.EnsureOnSale();
        var plan = new Plan(product.Id, product.Name, quantity, intervalDays, product.Price);
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

        Raise(new SubscriptionActivated(Id, CustomerId, Plan.ProductId, now));
    }

    public void Pause(DateTime now)
    {
        RejectUnless(Status == SubscriptionStatus.Active,
            "Only an active subscription can be paused.");

        Status = SubscriptionStatus.Paused;
        PausedAt = now;
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

        Raise(new SubscriptionCancelled(Id, now));

        // Only what was merely planned. A delivery an order was already
        // placed for runs its own course and can still arrive; stopping that
        // parcel means cancelling the order, not the subscription.
        foreach (var delivery in _deliveries.Where(d => d.IsScheduled).ToList())
        {
            delivery.Cancel();
        }
    }

    public void ChangePlan(OfferedProduct product, Quantity quantity, int intervalDays)
    {
        product.EnsureOnSale();
        var plan = new Plan(product.Id, product.Name, quantity, intervalDays, product.Price);
        RejectUnless(Status is SubscriptionStatus.Active or SubscriptionStatus.Paused,
            "Only a running subscription can change its plan.");

        if (plan.ProductId != Plan.ProductId)
        {
            Raise(new SubscriptionProductChanged(Id, plan.ProductId));
        }

        Plan = plan;

        // Deliveries already being fulfilled keep what was agreed for them;
        // the ones still only planned follow the new plan but keep their
        // date. The new interval counts from the next delivery on.
        foreach (var delivery in _deliveries.Where(d => d.IsScheduled).ToList())
        {
            delivery.FollowPlan(plan);
        }
    }

    public void SkipNextDelivery(DateTime now)
    {
        RejectUnless(Status == SubscriptionStatus.Active,
            "Only an active subscription can skip a delivery.");

        var next = NextScheduled()
                   ?? throw new DomainRuleException("There is no delivery left to skip.");

        next.Skip();
        Schedule(next.ScheduledFor.AddDays(Plan.IntervalDays));
    }

    public void AddExtra(Guid deliveryId, OfferedProduct product, Quantity quantity)
    {
        product.EnsureOnSale();
        EditableDelivery(deliveryId).AddExtra(product.Id, product.Name, quantity, product.Price);
    }

    public void RemoveExtra(Guid deliveryId, Guid productId)
    {
        EditableDelivery(deliveryId).RemoveExtra(productId);
    }

    // The delivery is due: it is announced with everything an order needs,
    // and waits as Processing for the outcome.
    public void StartDelivery(Guid deliveryId, DateTime now)
    {
        RejectUnless(Status == SubscriptionStatus.Active, "Only an active subscription can prepare a delivery.");
        var delivery = EditableDelivery(deliveryId);
        RejectUnless(delivery.ScheduledFor <= now, "This delivery is not due yet.");
        delivery.StartProcessing();

        var lines = new List<DeliveryLine> { new(delivery.ProductId, Plan.ProductName, delivery.Quantity, delivery.UnitPrice) };
        lines.AddRange(delivery.Extras.Select(e => new DeliveryLine(e.ProductId, e.ProductName, e.Quantity, e.UnitPrice)));

        Raise(new DeliveryDue(Id, deliveryId, CustomerId, lines, now));
    }

    // The order for the delivery is paid. Hearing it again changes nothing.
    public void CompleteDelivery(Guid deliveryId, Guid orderId)
    {
        var delivery = DeliveryOf(deliveryId);
        if (delivery.Status == DeliveryStatus.Ready && delivery.OrderId == orderId)
        {
            return;
        }

        delivery.Complete(orderId);
        ScheduleNextAfter(delivery);
    }

    // No order came of the delivery. A business decision, not a rollback: the
    // delivery is skipped and the next one comes as planned. Hearing it again
    // changes nothing.
    public void SkipFailedDelivery(Guid deliveryId, Guid? orderId, string reason)
    {
        var delivery = DeliveryOf(deliveryId);
        if (delivery.Status == DeliveryStatus.Skipped)
        {
            return;
        }

        delivery.Fail(orderId);
        ScheduleNextAfter(delivery);
        Raise(new DeliverySkipped(Id, deliveryId, reason));
    }

    private void ScheduleNextAfter(Delivery delivery)
    {
        // A cancelled subscription plans nothing more, and one resumed while
        // this delivery was processing already has its next one.
        if (Status != SubscriptionStatus.Cancelled && NextScheduled() == null)
        {
            Schedule(delivery.ScheduledFor.AddDays(Plan.IntervalDays));
        }
    }

    private Delivery DeliveryOf(Guid deliveryId) =>
        _deliveries.SingleOrDefault(d => d.Id == deliveryId)
        ?? throw new DomainRuleException("This subscription has no such delivery.");

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
