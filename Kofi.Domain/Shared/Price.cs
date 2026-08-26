namespace Kofi.Domain.Shared;

// The constructor is the only way to make a price, and it refuses one that
// cannot exist. Nothing that receives a Price has to check it again.
public record Price
{
    // numeric(18,2) stores sixteen whole-number digits and two decimal places.
    public const decimal MaxStoredAmount = 9999999999999999.99m;

    public Price(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainRuleException("A price needs a currency.");
        }

        if (amount < 0)
        {
            throw new DomainRuleException("A price cannot be negative.");
        }

        if (amount > MaxStoredAmount)
        {
            throw new DomainRuleException("A price is larger than we can store.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new DomainRuleException("A price has at most two decimal places.");
        }

        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Price operator +(Price left, Price right)
    {
        if (left.Currency != right.Currency)
        {
            throw new DomainRuleException(
                $"Cannot add {left.Currency} to {right.Currency}. Converting between "
                + "currencies is a decision of its own, not an implicit sum.");
        }

        return new Price(left.Amount + right.Amount, left.Currency);
    }

    public static Price operator *(Price price, int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainRuleException("A price is multiplied only by a positive quantity.");
        }

        return new Price(price.Amount * quantity, price.Currency);
    }

    public override string ToString() => $"{Amount} {Currency}";
}
