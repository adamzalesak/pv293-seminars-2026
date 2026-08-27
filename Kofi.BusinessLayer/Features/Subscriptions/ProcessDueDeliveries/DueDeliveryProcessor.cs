using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Kofi.BusinessLayer.Abstractions;
using Kofi.Domain;
using Kofi.Domain.Subscriptions;
using Kofi.DataAccessLayer.Repositories;
using Kofi.BusinessLayer.Features.Subscriptions.PrepareDelivery;

namespace Kofi.BusinessLayer.Features.Subscriptions.ProcessDueDeliveries;

// What the daily cron job in our infrastructure calls. Every due delivery goes
// through the same PrepareDeliveryCommand as a manual /prepare, each in its
// own scope: its own DbContext, its own transaction. A delivery that is turned
// down (no stock, a product gone) stays Scheduled for the next run and does
// not stop the others. Every due delivery is tried once per run, oldest first:
// a batch limit would let the same failing ones crowd out the rest forever.
public class DueDeliveryProcessor(ISubscriptionRepository subscriptions, IServiceScopeFactory scopes)
{
    public async Task<ProcessDueDeliveriesResponse> Run(CancellationToken ct)
    {
        var now = DateTime.Now;
        var rows = await subscriptions.Query()
            .Where(s => s.Status == SubscriptionStatus.Active
                && s.Deliveries.Any(d => d.Status == DeliveryStatus.Scheduled && d.ScheduledFor <= now))
            .Select(s => new
            {
                SubscriptionId = s.Id,
                Due = s.Deliveries
                    .Where(d => d.Status == DeliveryStatus.Scheduled && d.ScheduledFor <= now)
                    .Select(d => new { d.Id, d.ScheduledFor })
                    .ToList(),
            })
            .ToListAsync(ct);
        var due = rows
            .SelectMany(r => r.Due, (r, d) => new { r.SubscriptionId, DeliveryId = d.Id, d.ScheduledFor })
            .OrderBy(d => d.ScheduledFor)
            .ToList();

        var prepared = new List<Guid>();
        var failed = new List<DueDeliveryFailure>();
        foreach (var delivery in due)
        {
            await using var scope = scopes.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            try
            {
                await mediator.Send(new PrepareDeliveryCommand
                {
                    SubscriptionId = delivery.SubscriptionId, DeliveryId = delivery.DeliveryId,
                }, ct);
                prepared.Add(delivery.DeliveryId);
            }
            catch (Exception ex) when (ex is DomainException or DomainRuleException or NotFoundException)
            {
                failed.Add(new DueDeliveryFailure { DeliveryId = delivery.DeliveryId, Reason = ex.Message });
            }
        }

        return new ProcessDueDeliveriesResponse { Prepared = prepared, Failed = failed };
    }
}
