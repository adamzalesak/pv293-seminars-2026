using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.ResendOrderConfirmation;

public record ResendOrderConfirmationCommand : ICommand
{
    public Guid Id { get; init; }
}
