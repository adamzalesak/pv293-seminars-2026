using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.ExportOrders;

public record ExportOrdersQuery : IQuery<string>
{
    public DateTime? From { get; init; }
}
