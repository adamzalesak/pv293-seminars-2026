using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.RemoveItemFromOrder;

public record RemoveItemFromOrderCommand : ICommand
{
    public Guid OrderId { get; init; }
    public Guid OrderItemId { get; init; }
}
