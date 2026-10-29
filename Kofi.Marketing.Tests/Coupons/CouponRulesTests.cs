using Kofi.Common.Domain;
using Kofi.Marketing.Domain.Coupons;
using Xunit;

namespace Kofi.Marketing.Tests.Coupons;

public class CouponRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 7, 12, 0, 0);
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Friend = Guid.NewGuid();

    [Fact]
    public void Referral_code_is_for_friends_once_each()
    {
        var coupon = Coupon.IssueReferral(new CouponCode("ref-abc123"), Owner, Guid.NewGuid(), Now);

        Assert.Equal("REF-ABC123", coupon.Code.Value);
        Assert.Throws<DomainRuleException>(() => coupon.EnsureUsableBy(Owner, Now));

        coupon.EnsureUsableBy(Friend, Now);
        coupon.RecordUse(Friend, Guid.NewGuid(), Now);
        Assert.Throws<DomainRuleException>(() => coupon.EnsureUsableBy(Friend, Now));

        coupon.RecordUse(Guid.NewGuid(), Guid.NewGuid(), Now);
        Assert.Equal(2, coupon.Uses.Count);
    }

    [Fact]
    public void A_paid_order_records_its_use_once_even_against_the_rules()
    {
        var coupon = Coupon.IssueReferral(new CouponCode("REF-ABC123"), Owner, Guid.NewGuid(), Now);
        var order = Guid.NewGuid();

        coupon.RecordUse(Friend, order, Now);
        coupon.RecordUse(Friend, order, Now);
        coupon.RecordUse(Friend, Guid.NewGuid(), Now);

        Assert.Equal(2, coupon.Uses.Count);
    }

    [Fact]
    public void Expired_or_invalidated_coupon_cannot_be_used()
    {
        var coupon = Coupon.Issue(new CouponCode("KOFI10"), new Discount(10), Now.AddDays(1), Now);
        Assert.Throws<DomainRuleException>(() => coupon.EnsureUsableBy(Friend, Now.AddDays(2)));

        coupon.Invalidate();
        Assert.Throws<DomainRuleException>(() => coupon.EnsureUsableBy(Friend, Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Discount_stays_in_range(int percent)
    {
        Assert.Throws<DomainRuleException>(() => new Discount(percent));
    }
}
