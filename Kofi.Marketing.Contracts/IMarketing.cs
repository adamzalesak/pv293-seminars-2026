namespace Kofi.Marketing.Contracts;

public interface IMarketing
{
    Task IssueReferralCoupon(Guid customerId, CancellationToken ct = default);

    // Marketing decides who may use a code; the caller only learns the discount.
    Task<CouponDiscount> ValidateCoupon(string code, Guid customerId, CancellationToken ct = default);

    Task RedeemCoupon(string code, Guid customerId, Guid orderId, CancellationToken ct = default);
}
