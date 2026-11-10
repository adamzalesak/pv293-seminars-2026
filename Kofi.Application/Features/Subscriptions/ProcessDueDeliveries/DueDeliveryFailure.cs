namespace Kofi.Application.Features.Subscriptions.ProcessDueDeliveries;

public record DueDeliveryFailure
{
    public required Guid DeliveryId { get; init; }
    public required string Reason { get; init; }
}
