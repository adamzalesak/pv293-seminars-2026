using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.GetOrderCountByStatus;

public record GetOrderCountByStatusQuery : IQuery<int>
{
    public string Status { get; init; } = "";
}
