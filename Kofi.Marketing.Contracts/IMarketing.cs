namespace Kofi.Marketing.Contracts;

public interface IMarketing
{
    // Marketing decides who may use a code; the caller only learns the discount.
    Task<CouponDiscount> ValidateCoupon(string code, Guid customerId, CancellationToken ct = default);
}
