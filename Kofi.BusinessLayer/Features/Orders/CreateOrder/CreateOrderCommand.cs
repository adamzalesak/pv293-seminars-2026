using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.CreateOrder;

public record CreateOrderCommand : ICommand<CreateOrderResponse>
{
    public string CustomerName { get; init; } = "";
    public string Street { get; init; } = "";
    public string City { get; init; } = "";
    public string Zip { get; init; } = "";
    public List<CreateOrderItem> Items { get; init; } = new();
}
