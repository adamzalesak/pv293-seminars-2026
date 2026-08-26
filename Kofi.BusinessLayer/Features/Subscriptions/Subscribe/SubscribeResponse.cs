namespace Kofi.BusinessLayer.Features.Subscriptions.Subscribe;

public record SubscribeResponse
{
    public required Guid Id { get; init; }
    public required string Status { get; init; }
}
