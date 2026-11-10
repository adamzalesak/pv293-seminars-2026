using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Subscriptions.Subscribe;

public record SubscribeCommand : ICommand<SubscribeResponse>
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public int IntervalDays { get; init; }
    public DateOnly FirstDeliveryOn { get; init; }
}
