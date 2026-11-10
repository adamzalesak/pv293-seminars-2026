using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.CancelOrder;

public record CancelOrderCommand : ICommand
{
    public Guid Id { get; init; }
}
