using Kofi.Common.Domain;
using MediatR;

namespace Kofi.Host;

// Domain events of the command, stored by Wolverine in the same transaction as the data.
public class DomainEventBehavior<TRequest, TResponse>(ModuleUnitOfWork work)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next(ct);

        var aggregates = work.Contexts.SelectMany(c => c.ChangeTracker.Entries<AggregateRoot>())
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();
        if (aggregates.Count == 0)
        {
            return response;
        }

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        work.Publish(events);

        return response;
    }
}
