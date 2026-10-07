namespace Kofi.Ordering.Domain.Orders;

// What Marketing said the code is worth when the order was placed. The order
// keeps it like its item prices, so a later change to the coupon does not
// change what the customer pays.
public record AppliedCoupon
{
    private AppliedCoupon()
    {
    }

    public AppliedCoupon(string code, int discountPercent)
    {
        if (discountPercent is < 1 or > 100)
        {
            throw new DomainRuleException("A discount is a percentage.");
        }

        Code = code;
        DiscountPercent = discountPercent;
    }

    public string Code { get; private init; } = "";
    public int DiscountPercent { get; private init; }
}
