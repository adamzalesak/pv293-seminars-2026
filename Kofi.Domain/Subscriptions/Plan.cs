using Kofi.Domain.Shared;

namespace Kofi.Domain.Subscriptions;

// What the customer agreed to: which coffee, how many packs, how often, and
// at what price. It has no identity of its own — changing the plan replaces
// the whole value.
public record Plan
{
    // EF materializes the value field by field. The constructor below is the
    // only way the application can make one.
    private Plan()
    {
    }

    public Plan(Guid productId, string productName, Quantity quantity, int intervalDays, Price unitPrice)
    {
        if (intervalDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervalDays), "A subscription needs an interval.");
        }

        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        IntervalDays = intervalDays;
        UnitPrice = unitPrice;
    }

    public Guid ProductId { get; }
    public string ProductName { get; } = "";
    public Quantity Quantity { get; } = null!;
    public int IntervalDays { get; }
    public Price UnitPrice { get; } = null!;

    public Price LineTotal => UnitPrice * Quantity;
}
