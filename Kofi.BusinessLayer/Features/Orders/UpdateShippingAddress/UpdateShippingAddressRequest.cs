namespace Kofi.BusinessLayer.Features.Orders.UpdateShippingAddress;

public record UpdateShippingAddressRequest
{
    public string Street { get; init; } = "";
    public string City { get; init; } = "";
    public string Zip { get; init; } = "";
}
