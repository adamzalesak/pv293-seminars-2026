using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Kofi.Application.Services;
using Kofi.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.EventHandlers;

// The reaction, not the decision: activation happens in the aggregate, the
// welcome e-mail follows from it here. It only reads, so it projects what the
// e-mail needs instead of loading the aggregate.
public class SendWelcomeEmailOnSubscriptionActivated(IApplicationDbContext db, IEmailService email)
    : IDomainEventHandler<SubscriptionActivated>
{
    public async Task Handle(SubscriptionActivated activated, CancellationToken ct)
    {
        var welcome = await db.Subscriptions
                          .Where(s => s.Id == activated.SubscriptionId)
                          .Join(db.Customers, s => s.CustomerId, c => c.Id, (s, c) => new WelcomeEmail(
                              s.Id, c.Email.Value, s.Plan.ProductName, s.Plan.Quantity.Value, s.Plan.IntervalDays))
                          .SingleOrDefaultAsync(ct)
                      ?? throw new NotFoundException("Subscription not found");

        await email.SendSubscriptionActivated(welcome);
    }
}
