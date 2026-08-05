using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.MarkOrderShipped;

public record MarkOrderShippedCommand : ICommand
{
    public Guid Id { get; init; }
}
