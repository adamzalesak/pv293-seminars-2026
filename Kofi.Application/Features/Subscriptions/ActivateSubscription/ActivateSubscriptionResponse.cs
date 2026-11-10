namespace Kofi.Application.Features.Subscriptions.ActivateSubscription;

public record ActivateSubscriptionResponse
{
    public required string Status { get; init; }
    public required string? CardSetupStatus { get; init; }
}
