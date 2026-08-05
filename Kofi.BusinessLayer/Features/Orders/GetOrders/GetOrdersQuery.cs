using Microsoft.EntityFrameworkCore;
using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.GetOrders;

public record GetOrdersQuery : IQuery<List<OrderSummary>>
{
    public bool? OnlyPaid { get; init; }
    public DateTime? From { get; init; }
    public string? SortBy { get; init; }
}
