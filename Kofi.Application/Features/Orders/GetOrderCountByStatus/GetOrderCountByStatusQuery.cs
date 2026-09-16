using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.GetOrderCountByStatus;

public record GetOrderCountByStatusQuery : IQuery<int>
{
    public string Status { get; init; } = "";
}
