using Kofi.Common.Abstractions;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.StartCardSetup;

public record StartCardSetupCommand : ICommand<StartCardSetupResponse>, IForSubscription
{
    public Guid Id { get; init; }
    public string ReturnUrl { get; init; } = "";
    Guid IForSubscription.SubscriptionId => Id;
}
