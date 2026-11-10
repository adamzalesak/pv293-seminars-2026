using Kofi.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> product)
    {
        product.ToTable("Products");
        product.HasKey(p => p.Id);
        product.Ignore(p => p.DomainEvents);
        product.ComplexProperty(p => p.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("Price").HasPrecision(18, 2);
            price.Property(p => p.Currency).HasColumnName("PriceCurrency");
        });
    }
}
