using Kofi.Application.Abstractions;
using Kofi.Application.Features.Subscriptions;
using Kofi.Application.Persistence;
using Kofi.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Behaviors;

// A customer may only see and change their own subscription. Someone else's
// looks exactly like one that does not exist. The administrator sees them all.
public class SubscriptionOwnershipBehavior<TRequest, TResponse>(
    ICurrentUser user,
    IApplicationDbContext db)
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
            var owner = await db.Subscriptions
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
