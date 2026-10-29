using Wolverine.Attributes;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.ProcessDueDeliveries;

// The daily run, now started by Wolverine instead of a cron job outside the
// application. The processor gives every delivery its own scope and
// transaction, so this handler needs none of its own.
[NonTransactional]
public class PrepareDueDeliveriesHandler(DueDeliveryProcessor processor)
{
    public Task Handle(PrepareDueDeliveries message, CancellationToken ct) => processor.Run(ct);
}
