using Kofi.Common.Domain;
using MediatR;

namespace Kofi.Host;

public class DomainEventBehavior<TRequest, TResponse>(ModuleUnitOfWork work, IPublisher publisher)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next(ct);

        while (true)
        {
            var aggregates = work.Contexts.SelectMany(c => c.ChangeTracker.Entries<AggregateRoot>())
                .Select(e => e.Entity)
                .Where(a => a.DomainEvents.Count > 0)
                .ToList();
            if (aggregates.Count == 0)
            {
                break;
            }

            var events = aggregates.SelectMany(a => a.DomainEvents).ToList();

            foreach (var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            foreach (var domainEvent in events)
            {
                await publisher.Publish(domainEvent, ct);
            }
        }

        return response;
    }
}
