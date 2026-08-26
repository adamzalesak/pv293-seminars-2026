namespace Kofi.BusinessLayer.Features.Subscriptions.ChangePlan;

public record ChangePlanRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public int IntervalDays { get; init; }
}
