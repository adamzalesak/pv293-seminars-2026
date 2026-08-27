using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.PauseSubscription;

public class PauseSubscriptionHandler(ISubscriptionRepository subscriptions) : ICommandHandler<PauseSubscriptionCommand>
{
    public async Task Handle(PauseSubscriptionCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
            ?? throw new NotFoundException("Subscription not found");

        subscription.Pause(DateTime.Now);
    }
}
