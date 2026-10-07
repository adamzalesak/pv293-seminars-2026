using Kofi.Marketing.Domain.Coupons;

namespace Kofi.Marketing.Infrastructure.Persistence;

internal static class DemoData
{
    private static readonly Guid Petra = Guid.Parse("6f1c2a4e-8d3b-4c7a-9e21-5b0f3d8a7c14");

    public static void Seed(MarketingDbContext db)
    {
        if (db.Coupons.Any())
        {
            return;
        }

        var now = DateTime.Now;
        db.Coupons.AddRange(
            Coupon.IssueReferral(new CouponCode("REF-PETRA7"), Petra, now.AddDays(-20)),
            Coupon.Issue(new CouponCode("KOFI10"), new Discount(10), now.AddDays(30),
                now.AddDays(-5)));
        db.SaveChanges();
    }
}
