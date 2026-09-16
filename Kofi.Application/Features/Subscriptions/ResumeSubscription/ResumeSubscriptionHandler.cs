using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;

namespace Kofi.Application.Features.Subscriptions.ResumeSubscription;

public class ResumeSubscriptionHandler(ISubscriptionRepository subscriptions) : ICommandHandler<ResumeSubscriptionCommand>
{
    public async Task Handle(ResumeSubscriptionCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
                           ?? throw new NotFoundException("Subscription not found");

        subscription.Resume(command.NextDeliveryOn.ToDateTime(TimeOnly.MinValue), DateTime.Now);
    }
}
