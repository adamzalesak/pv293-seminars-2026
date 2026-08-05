using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.ResendOrderConfirmation;

public record ResendOrderConfirmationCommand : ICommand
{
    public Guid Id { get; init; }
}
