namespace Kofi.Marketing.Domain.Coupons;

public class CouponUse
{
    private CouponUse()
    {
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid OrderId { get; private set; }
    public DateTime UsedAt { get; private set; }

    internal static CouponUse Create(Guid customerId, Guid orderId, DateTime now) =>
        new() { Id = Guid.NewGuid(), CustomerId = customerId, OrderId = orderId, UsedAt = now };
}
