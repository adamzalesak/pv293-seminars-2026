using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.GetOrderCountByStatus;

public record GetOrderCountByStatusQuery : IQuery<int>
{
    public string Status { get; init; } = "";
}
