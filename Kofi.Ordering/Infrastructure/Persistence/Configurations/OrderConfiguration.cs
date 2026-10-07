using Kofi.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Ordering.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> order)
    {
        order.ComplexProperty(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street");
            address.Property(a => a.City).HasColumnName("City");
            address.Property(a => a.Zip).HasColumnName("Zip");
        });
        order.ToTable("Orders", "ordering");
        order.HasKey(o => o.Id);
        order.Ignore(o => o.DomainEvents);
        order.Property(o => o.Status).HasConversion<string>();
        order.ComplexProperty(o => o.CustomerEmail, email => email.Property(e => e.Value).HasColumnName("CustomerEmail"));
        order.OwnsMany(o => o.Items, item =>
        {
            item.ToTable("OrderItems", "ordering");
            item.WithOwner().HasForeignKey("OrderId");
            item.HasKey(i => i.Id);
            item.Property(i => i.Id).ValueGeneratedNever();
            item.OwnsOne(i => i.Quantity, q => q.Property(v => v.Value).HasColumnName("Quantity"));
            item.Navigation(i => i.Quantity).IsRequired();
            item.OwnsOne(i => i.UnitPrice, price =>
            {
                price.Property(p => p.Amount).HasColumnName("UnitPrice").HasPrecision(18, 2);
                price.Property(p => p.Currency).HasColumnName("Currency");
            });
            item.Navigation(i => i.UnitPrice).IsRequired();
        });
        order.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        order.OwnsOne(o => o.Coupon, coupon =>
        {
            coupon.Property(c => c.Code).HasColumnName("CouponCode");
            coupon.Property(c => c.DiscountPercent).HasColumnName("CouponDiscountPercent");
        });
    }
}
