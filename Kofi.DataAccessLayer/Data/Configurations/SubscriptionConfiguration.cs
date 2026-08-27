using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Kofi.Domain.Subscriptions;

namespace Kofi.DataAccessLayer.Data.Configurations;

// What the aggregate owns is loaded with its root; what it only refers to
// gets a column with an id and no navigation property.
public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> subscription)
    {
        subscription.ToTable("Subscriptions");
        subscription.HasKey(s => s.Id);
        subscription.Property(s => s.Id).ValueGeneratedNever();
        subscription.Property(s => s.Status).HasConversion<string>();

        // The customer lives in another aggregate. We keep the id and nothing
        // else: no navigation means no way to reach across and change what we
        // do not own.
        subscription.Property(s => s.CustomerId).IsRequired();

        // The plan is a value, not an entity: no key of its own and no table
        // of its own, just columns of the subscription row.
        subscription.ComplexProperty(s => s.Plan, plan =>
        {
            plan.Property(p => p.ProductId).HasColumnName("ProductId");
            plan.Property(p => p.ProductName).HasColumnName("ProductName");
            plan.ComplexProperty(p => p.Quantity).Property(q => q.Value).HasColumnName("Quantity");
            plan.Property(p => p.IntervalDays).HasColumnName("IntervalDays");
            plan.ComplexProperty(p => p.UnitPrice, price =>
            {
                price.Property(m => m.Amount).HasColumnName("UnitPrice_Amount").HasPrecision(18, 2);
                price.Property(m => m.Currency).HasColumnName("UnitPrice_Currency").HasMaxLength(3);
            });
        });

        // EF writes the backing field, so the aggregate can keep handing out
        // a read-only view, and a query can still read the property.
        subscription.OwnsMany(s => s.Deliveries, delivery =>
        {
            delivery.ToTable("SubscriptionDeliveries");
            delivery.WithOwner().HasForeignKey("SubscriptionId");
            delivery.HasKey(d => d.Id);
            delivery.Property(d => d.Id).ValueGeneratedNever();
            delivery.Property(d => d.Status).HasConversion<string>();

            delivery.OwnsMany(d => d.Extras, extra =>
            {
                extra.ToTable("SubscriptionDeliveryExtras");
                extra.WithOwner().HasForeignKey("DeliveryId");
                extra.HasKey("DeliveryId", nameof(DeliveryExtra.ProductId));
                extra.Property(e => e.ProductId).ValueGeneratedNever();
                extra.OwnsOne(e => e.UnitPrice, price =>
                {
                    price.Property(m => m.Amount).HasColumnName("UnitPrice_Amount").HasPrecision(18, 2);
                    price.Property(m => m.Currency).HasColumnName("UnitPrice_Currency").HasMaxLength(3);
                });
                extra.Navigation(e => e.UnitPrice).IsRequired();
                extra.OwnsOne(e => e.Quantity, quantity => quantity.Property(q => q.Value).HasColumnName("Quantity"));
                extra.Navigation(e => e.Quantity).IsRequired();
            });
            delivery.Navigation(d => d.Extras).UsePropertyAccessMode(PropertyAccessMode.Field);

            // The same values, mapped the other way: inside an owned entity EF
            // offers no ComplexProperty, so the price and the quantity become
            // owned values with an identity they do not need. The model is
            // unchanged; this is only what the mapping API can express.
            delivery.OwnsOne(d => d.UnitPrice, price =>
            {
                price.Property(m => m.Amount).HasColumnName("UnitPrice_Amount").HasPrecision(18, 2);
                price.Property(m => m.Currency).HasColumnName("UnitPrice_Currency").HasMaxLength(3);
            });
            delivery.Navigation(d => d.UnitPrice).IsRequired();
            delivery.OwnsOne(d => d.Quantity, quantity => quantity.Property(q => q.Value).HasColumnName("Quantity"));
            delivery.Navigation(d => d.Quantity).IsRequired();
        });
        subscription.Navigation(s => s.Deliveries).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Facts waiting to be published, not state of the subscription.
        subscription.Ignore(s => s.DomainEvents);
    }
}
