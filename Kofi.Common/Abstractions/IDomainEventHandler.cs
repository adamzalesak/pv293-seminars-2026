using Kofi.Common.Domain;
using MediatR;

namespace Kofi.Common.Abstractions;

public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent;
