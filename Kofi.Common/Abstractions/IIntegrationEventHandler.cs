using Kofi.Contracts;

namespace Kofi.Common.Abstractions;

// Wolverine runs every implementation as its own handler, from a durable local queue.
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task Handle(TEvent integrationEvent, CancellationToken ct);
}
