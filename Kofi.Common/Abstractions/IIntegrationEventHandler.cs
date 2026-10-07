using Kofi.Contracts;
using MediatR;

namespace Kofi.Common.Abstractions;

public interface IIntegrationEventHandler<in TEvent> : INotificationHandler<TEvent>
    where TEvent : IIntegrationEvent;
