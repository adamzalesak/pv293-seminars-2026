using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.GetOrderDetail;

public record GetOrderDetailQuery : IQuery<OrderDetail?>
{
    public Guid Id { get; init; }
}
