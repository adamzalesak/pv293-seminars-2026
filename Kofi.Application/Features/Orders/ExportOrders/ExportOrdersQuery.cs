using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.ExportOrders;

public record ExportOrdersQuery : IQuery<string>
{
    public DateTime? From { get; init; }
}
