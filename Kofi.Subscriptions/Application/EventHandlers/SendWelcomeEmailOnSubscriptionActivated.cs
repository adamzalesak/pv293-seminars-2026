using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Contracts;
using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Subscriptions.Application.EventHandlers;

public class SendWelcomeEmailOnSubscriptionActivated(ISubscriptionsDbContext db, IOrdering ordering, IEmailTransport email)
    : IDomainEventHandler<SubscriptionActivated>
{
    public async Task Handle(SubscriptionActivated activated, CancellationToken ct)
    {
        var sub = await db.Subscriptions.Where(s => s.Id == activated.SubscriptionId)
            .Select(s => new { s.Id, s.CustomerId, s.Plan.ProductName, Quantity = s.Plan.Quantity.Value, s.Plan.IntervalDays })
            .SingleAsync(ct);

        var customer = await ordering.GetCustomer(sub.CustomerId, ct) ?? throw new NotFoundException("Customer not found");
        await email.Send(customer.Email, "Your subscription is active",
            $"Subscription {sub.Id} is active: {sub.Quantity} x {sub.ProductName} every {sub.IntervalDays} days.");
    }
}
