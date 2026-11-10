using Kofi.Domain.Shared;

namespace Kofi.Domain.Orders;

public class OrderItem
{
    private OrderItem()
    {
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = "";
    public Price UnitPrice { get; private set; } = null!;
    public Quantity Quantity { get; private set; } = null!;

    public static OrderItem Create(Guid productId, string productName, Quantity quantity, Price price) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = productId,
        ProductName = productName,
        Quantity = new Quantity(quantity.Value),
        UnitPrice = new Price(price.Amount, price.Currency),
    };
}
