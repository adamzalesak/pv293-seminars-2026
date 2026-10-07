namespace Kofi.Marketing.Domain.Coupons;

public class Coupon : AggregateRoot
{
    public static readonly Discount ReferralDiscount = new(15);
    public static readonly TimeSpan ReferralValidity = TimeSpan.FromDays(90);

    private readonly List<CouponUse> _uses = new();

    private Coupon()
    {
    }

    public CouponCode Code { get; private set; } = null!;
    public CouponReason Reason { get; private set; }

    // Whoever got a referral code passes it on; they do not use it themselves.
    public Guid? OwnerCustomerId { get; private set; }

    public Discount Discount { get; private set; } = null!;
    public DateTime IssuedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsInvalidated { get; private set; }
    public IReadOnlyList<CouponUse> Uses => _uses.AsReadOnly();

    public static Coupon Issue(CouponCode code, Discount discount, DateTime expiresAt, DateTime now)
    {
        var coupon = new Coupon { Id = Guid.NewGuid(), Code = code, Reason = CouponReason.Manual, IssuedAt = now };
        coupon.SetTerms(discount, expiresAt, now);
        return coupon;
    }

    public static Coupon IssueReferral(CouponCode code, Guid ownerCustomerId, DateTime now)
    {
        var coupon = new Coupon
            { Id = Guid.NewGuid(), Code = code, Reason = CouponReason.Referral, OwnerCustomerId = ownerCustomerId, IssuedAt = now };
        coupon.SetTerms(ReferralDiscount, now.Add(ReferralValidity), now);
        return coupon;
    }

    public void Update(Discount discount, DateTime expiresAt, DateTime now)
    {
        EnsureNotInvalidated();
        SetTerms(discount, expiresAt, now);
    }

    public void Invalidate()
    {
        EnsureNotInvalidated();
        IsInvalidated = true;
    }

    public void EnsureUsableBy(Guid customerId, DateTime now)
    {
        if (IsInvalidated || ExpiresAt <= now)
        {
            throw new DomainRuleException("This coupon is no longer valid.");
        }

        if (OwnerCustomerId == customerId)
        {
            throw new DomainRuleException("A referral code is for someone else.");
        }

        if (_uses.Any(u => u.CustomerId == customerId))
        {
            throw new DomainRuleException("This coupon has already been used.");
        }
    }

    public void Redeem(Guid customerId, Guid orderId, DateTime now)
    {
        EnsureUsableBy(customerId, now);
        _uses.Add(CouponUse.Create(customerId, orderId, now));
    }

    private void SetTerms(Discount discount, DateTime expiresAt, DateTime now)
    {
        if (expiresAt <= now)
        {
            throw new DomainRuleException("A coupon has to expire in the future.");
        }

        Discount = discount;
        ExpiresAt = expiresAt;
    }

    private void EnsureNotInvalidated()
    {
        if (IsInvalidated)
        {
            throw new DomainRuleException("This coupon has been invalidated.");
        }
    }
}
