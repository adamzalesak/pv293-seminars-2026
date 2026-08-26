using MediatR;
using Microsoft.EntityFrameworkCore;
using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Features.Subscriptions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Behaviors;

// A customer may only see and change their own subscription. Someone else's
// looks exactly like one that does not exist. The administrator sees them all.
public class SubscriptionOwnershipBehavior<TRequest, TResponse>(
    ICurrentUser user,
    ISubscriptionRepository subscriptions)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IForSubscription forSubscription && user.CustomerId != null)
        {
            var owner = await subscriptions.Query()
                .Where(s => s.Id == forSubscription.SubscriptionId)
                .Select(s => (Guid?)s.CustomerId)
                .SingleOrDefaultAsync(cancellationToken);
            if (owner != user.CustomerId)
            {
                throw new NotFoundException("Subscription not found");
            }
        }

        return await next(cancellationToken);
    }
}
