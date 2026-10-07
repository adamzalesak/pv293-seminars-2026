namespace Kofi.Warehouse.Domain.Products;

public class Product : AggregateRoot
{
    private Product()
    {
    }

    public Guid ProductId => Id;
    public int StockQuantity { get; private set; }
    public string ShelfLocation { get; private set; } = "";

    public static Product Create(Guid productId, int quantity, string shelfLocation)
    {
        if (quantity < 0)
        {
            throw new DomainRuleException("Stock cannot be negative.");
        }

        return new Product { Id = productId, StockQuantity = quantity, ShelfLocation = shelfLocation };
    }

    public void Restock(Quantity quantity) => StockQuantity += quantity.Value;

    public void Reserve(Quantity quantity)
    {
        if (quantity.Value > StockQuantity)
        {
            throw new DomainRuleException("Not enough stock.");
        }

        StockQuantity -= quantity.Value;
    }

    public void Release(Quantity quantity) => StockQuantity += quantity.Value;
}
