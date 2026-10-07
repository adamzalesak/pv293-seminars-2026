using MediatR;

namespace Kofi.Contracts;

// The marker comes from MediatR.Contracts, which holds only interfaces. Every
// Contracts project can reference it without pulling in the shared kernel.
public interface IIntegrationEvent : INotification;
