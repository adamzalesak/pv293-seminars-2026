using Kofi.Warehouse.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Warehouse.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> product)
    {
        product.ToTable("Products", "warehouse");
        product.HasKey(p => p.Id);
        product.Property(p => p.Id).HasColumnName("ProductId").ValueGeneratedNever();
        product.Ignore(p => p.ProductId);
        product.Ignore(p => p.DomainEvents);
        product.Property(p => p.ShelfLocation).IsRequired();
    }
}
