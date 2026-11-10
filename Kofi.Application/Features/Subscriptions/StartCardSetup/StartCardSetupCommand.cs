using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Subscriptions.StartCardSetup;

public record StartCardSetupCommand : ICommand<StartCardSetupResponse>, IForSubscription
{
    public Guid Id { get; init; }
    public string ReturnUrl { get; init; } = "";
    Guid IForSubscription.SubscriptionId => Id;
}
