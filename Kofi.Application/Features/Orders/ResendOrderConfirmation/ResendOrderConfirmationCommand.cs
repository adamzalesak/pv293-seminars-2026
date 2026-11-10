using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.ResendOrderConfirmation;

public record ResendOrderConfirmationCommand : ICommand
{
    public Guid Id { get; init; }
}
