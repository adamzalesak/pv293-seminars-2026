using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.GetOrderDetail;

public record GetOrderDetailQuery : IQuery<OrderDetail?>
{
    public Guid Id { get; init; }
}
