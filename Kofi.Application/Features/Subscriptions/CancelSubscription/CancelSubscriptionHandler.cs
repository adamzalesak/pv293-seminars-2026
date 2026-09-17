using Kofi.Application.Repositories;
using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Subscriptions.CancelSubscription;

public class CancelSubscriptionHandler(ISubscriptionRepository subscriptions) : ICommandHandler<CancelSubscriptionCommand>
{
    public async Task Handle(CancelSubscriptionCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
                           ?? throw new NotFoundException("Subscription not found");

        subscription.Cancel(DateTime.Now);
    }
}
