using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Repositories;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.SkipDelivery;

public class SkipDeliveryHandler(ISubscriptionRepository subscriptions) : ICommandHandler<SkipDeliveryCommand>
{
    public async Task Handle(SkipDeliveryCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
                           ?? throw new NotFoundException("Subscription not found");

        subscription.SkipNextDelivery(DateTime.Now);
    }
}
