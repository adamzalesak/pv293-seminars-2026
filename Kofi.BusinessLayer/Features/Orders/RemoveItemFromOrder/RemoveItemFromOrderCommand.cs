using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.RemoveItemFromOrder;

public record RemoveItemFromOrderCommand : ICommand
{
    public Guid OrderId { get; init; }
    public Guid OrderItemId { get; init; }
}
