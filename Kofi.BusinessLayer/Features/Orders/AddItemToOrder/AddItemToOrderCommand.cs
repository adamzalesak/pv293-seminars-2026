using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.AddItemToOrder;

public record AddItemToOrderCommand : ICommand
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
