using Kofi.Marketing.Domain.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Marketing.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> coupon)
    {
        coupon.ToTable("Coupons", "marketing");
        coupon.HasKey(c => c.Id);
        coupon.Property(c => c.Id).ValueGeneratedNever();
        coupon.Ignore(c => c.DomainEvents);
        coupon.OwnsOne(c => c.Code, code =>
        {
            code.Property(v => v.Value).HasColumnName("Code");
            code.HasIndex(v => v.Value).IsUnique();
        });
        coupon.Navigation(c => c.Code).IsRequired();
        coupon.OwnsOne(c => c.Discount, discount => discount.Property(d => d.Percent).HasColumnName("DiscountPercent"));
        coupon.Navigation(c => c.Discount).IsRequired();
        coupon.Property(c => c.Reason).HasConversion<string>();
        // One referral code per subscription, even when the event arrives twice at once.
        coupon.HasIndex(c => c.SubscriptionId).IsUnique().HasFilter("\"SubscriptionId\" IS NOT NULL");
        coupon.OwnsMany(c => c.Uses, use =>
        {
            use.ToTable("CouponUses", "marketing");
            use.WithOwner().HasForeignKey("CouponId");
            use.HasKey(u => u.Id);
            use.Property(u => u.Id).ValueGeneratedNever();
        });
        coupon.Navigation(c => c.Uses).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
