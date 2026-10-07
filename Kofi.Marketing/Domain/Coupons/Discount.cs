namespace Kofi.Marketing.Domain.Coupons;

public record Discount
{
    public const int MaxPercent = 50;

    public Discount(int percent)
    {
        if (percent is < 1 or > MaxPercent)
        {
            throw new DomainRuleException("A discount is between 1 and 50 %.");
        }

        Percent = percent;
    }

    public int Percent { get; }
}
