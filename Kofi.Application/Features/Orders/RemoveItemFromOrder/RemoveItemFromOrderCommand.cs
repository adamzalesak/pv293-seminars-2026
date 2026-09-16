using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.RemoveItemFromOrder;

public record RemoveItemFromOrderCommand : ICommand
{
    public Guid OrderId { get; init; }
    public Guid OrderItemId { get; init; }
}
