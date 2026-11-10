using Kofi.Application.Persistence;
using Kofi.Domain;
using MediatR;

namespace Kofi.Application.Behaviors;

// Runs after the handler and before SaveChanges: reactions that write to the
// same database land in the same unit of work and the same transaction.
// Sending an e-mail does not — a rollback will not take it back.
public class DomainEventBehavior<TRequest, TResponse>(IApplicationDbContext db, IPublisher publisher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        // A reaction may decide something of its own, so we keep publishing
        // until the aggregates have nothing left to say.
        while (Collect() is { Count: > 0 } events)
        {
            foreach (var domainEvent in events)
            {
                await publisher.Publish(domainEvent, cancellationToken);
            }
        }

        return response;
    }

    private List<IDomainEvent> Collect()
    {
        var aggregates = db.ChangeTracker.Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        return events;
    }
}
