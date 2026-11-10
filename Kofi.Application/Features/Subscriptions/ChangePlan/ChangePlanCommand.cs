using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Subscriptions.ChangePlan;

public record ChangePlanCommand : ICommand, IForSubscription
{
    public Guid Id { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public int IntervalDays { get; init; }
}
