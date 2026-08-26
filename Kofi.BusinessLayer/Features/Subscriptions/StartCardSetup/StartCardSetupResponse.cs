namespace Kofi.BusinessLayer.Features.Subscriptions.StartCardSetup;

public record StartCardSetupResponse
{
    public required Guid SetupId { get; init; }
    public required string RedirectUrl { get; init; }
}
