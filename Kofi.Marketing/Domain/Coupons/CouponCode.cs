using System.Text.RegularExpressions;

namespace Kofi.Marketing.Domain.Coupons;

// What a customer types at checkout. Stored in one spelling, so "kofi10" and
// "KOFI10" are the same coupon.
public record CouponCode
{
    public CouponCode(string value)
    {
        var code = (value ?? "").Trim().ToUpperInvariant();

        if (!Regex.IsMatch(code, "^[A-Z0-9-]{4,20}$"))
        {
            throw new DomainRuleException("A coupon code has 4 to 20 letters, digits or dashes.");
        }

        Value = code;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
