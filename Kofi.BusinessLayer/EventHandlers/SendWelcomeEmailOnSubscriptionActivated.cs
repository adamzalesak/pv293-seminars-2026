using Microsoft.EntityFrameworkCore;
using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Data;
using Kofi.Domain.Subscriptions;

namespace Kofi.BusinessLayer.EventHandlers;

// The reaction, not the decision: activation happens in the aggregate, the
// welcome e-mail follows from it here. It only reads, so it projects what the
// e-mail needs instead of loading the aggregate.
public class SendWelcomeEmailOnSubscriptionActivated(AppDbContext db, IEmailService email)
    : IDomainEventHandler<SubscriptionActivated>
{
    public async Task Handle(SubscriptionActivated activated, CancellationToken ct)
    {
        var welcome = await db.Subscriptions
            .Where(s => s.Id == activated.SubscriptionId)
            .Join(db.Customers, s => s.CustomerId, c => c.Id, (s, c) => new WelcomeEmail(
                s.Id, c.Email, s.Plan.ProductName, s.Plan.Quantity.Value, s.Plan.IntervalDays))
            .SingleOrDefaultAsync(ct)
            ?? throw new NotFoundException("Subscription not found");

        await email.SendSubscriptionActivated(welcome);
    }
}
