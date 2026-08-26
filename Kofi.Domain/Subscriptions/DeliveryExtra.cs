using Kofi.Domain.Shared;

namespace Kofi.Domain.Subscriptions;

// A one-time addition belongs to one delivery. Its accepted price is kept
// when more units of the same product are added later.
public class DeliveryExtra
{
    private DeliveryExtra()
    {
    }

    internal DeliveryExtra(Guid productId, string productName, Quantity quantity, Price unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = new Quantity(quantity.Value);
        UnitPrice = new Price(unitPrice.Amount, unitPrice.Currency);
    }

    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = "";
    public Quantity Quantity { get; private set; } = null!;
    public Price UnitPrice { get; private set; } = null!;

    internal void Increase(Quantity quantity) => Quantity += quantity;
}
