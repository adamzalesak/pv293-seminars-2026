using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.MarkOrderShipped;

public record MarkOrderShippedCommand : ICommand
{
    public Guid Id { get; init; }
}
