using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.CancelOrder;

public record CancelOrderCommand : ICommand
{
    public Guid Id { get; init; }
}
