using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.GetOrderDetail;

public record GetOrderDetailQuery : IQuery<OrderDetail?>
{
    public Guid Id { get; init; }
}
