using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.MarkOrderShipped;

public record MarkOrderShippedCommand : ICommand
{
    public Guid Id { get; init; }
}
