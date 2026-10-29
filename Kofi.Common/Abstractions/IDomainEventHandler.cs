using Kofi.Common.Domain;

namespace Kofi.Common.Abstractions;

// Wolverine runs every implementation as its own handler, from a durable local queue.
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken ct);
}
