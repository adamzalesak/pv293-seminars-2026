using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.PauseSubscription;

public class PauseSubscriptionHandler(ISubscriptionRepository subscriptions) : ICommandHandler<PauseSubscriptionCommand>
{
    public async Task Handle(PauseSubscriptionCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
            ?? throw new DomainException("Subscription not found");
        if (subscription.Status != SubscriptionStatus.Active)
            throw new DomainException("Only active subscriptions can be paused");

        subscription.Status = SubscriptionStatus.Paused;
        subscription.PausedAt = DateTime.Now;
    }
}
