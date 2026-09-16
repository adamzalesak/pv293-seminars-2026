namespace Kofi.Application.Features.Subscriptions.ProcessDueDeliveries;

public record ProcessDueDeliveriesResponse
{
    public required List<Guid> Prepared { get; init; }
    public required List<DueDeliveryFailure> Failed { get; init; }
}
