using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.CreateOrder;

public record CreateOrderCommand : ICommand<CreateOrderResponse>
{
    public string CustomerName { get; init; } = "";
    public string Street { get; init; } = "";
    public string City { get; init; } = "";
    public string Zip { get; init; } = "";
    public List<CreateOrderItem> Items { get; init; } = new();
}
