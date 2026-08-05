using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.CancelOrder;

public record CancelOrderCommand : ICommand
{
    public Guid Id { get; init; }
}
