namespace Kofi.Application.Features.Subscriptions.AddDeliveryExtra;

public record AddDeliveryExtraRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
