using Kofi.Catalog.Application.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Catalog.Infrastructure.Persistence.Configurations;

public class ProductSubscriptionConfiguration : IEntityTypeConfiguration<ProductSubscription>
{
    public void Configure(EntityTypeBuilder<ProductSubscription> subscription)
    {
        subscription.ToTable("ProductSubscriptions", "catalog");
        subscription.HasKey(s => s.SubscriptionId);
        subscription.Property(s => s.SubscriptionId).ValueGeneratedNever();
        subscription.HasIndex(s => s.ProductId);
    }
}
