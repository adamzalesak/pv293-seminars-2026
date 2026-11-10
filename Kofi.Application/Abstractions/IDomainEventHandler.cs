using Kofi.Domain;
using MediatR;

namespace Kofi.Application.Abstractions;

public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent;
