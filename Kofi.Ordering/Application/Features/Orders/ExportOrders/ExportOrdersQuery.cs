using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.ExportOrders;

public record ExportOrdersQuery : IQuery<string>
{
    public DateTime? From { get; init; }
}
