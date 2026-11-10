using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.UpdateShippingAddress;

public record UpdateShippingAddressCommand : ICommand
{
    public Guid Id { get; init; }
    public string Street { get; init; } = "";
    public string City { get; init; } = "";
    public string Zip { get; init; } = "";
}
