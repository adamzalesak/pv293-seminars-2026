using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Contracts;
using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Kofi.Subscriptions.Application.EventHandlers;

// Reads and sends; it reaches two modules' data, so no transaction of its own.
[NonTransactional]
public class SendEmailOnDeliverySkipped(ISubscriptionsDbContext db, IOrdering ordering, IEmailTransport email)
    : IDomainEventHandler<DeliverySkipped>
{
    public async Task Handle(DeliverySkipped skipped, CancellationToken ct)
    {
        var customerId = await db.Subscriptions.Where(s => s.Id == skipped.SubscriptionId).Select(s => s.CustomerId).SingleAsync(ct);
        var customer = await ordering.GetCustomer(customerId, ct) ?? throw new NotFoundException("Customer not found");
        await email.Send(customer.Email, "Your coffee delivery is skipped",
            $"We could not prepare your delivery: {skipped.Reason}. The next one comes as planned.");
    }
}
