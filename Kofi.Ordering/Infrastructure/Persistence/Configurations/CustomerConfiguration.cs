using Kofi.Ordering.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Ordering.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> customer)
    {
        customer.ComplexProperty(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street");
            address.Property(a => a.City).HasColumnName("City");
            address.Property(a => a.Zip).HasColumnName("Zip");
        });
        customer.ToTable("Customers", "public");
        customer.HasKey(c => c.Id);
        customer.Ignore(c => c.DomainEvents);
        customer.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email");
            email.HasIndex(e => e.Value).IsUnique();
        });
        customer.Navigation(c => c.Email).IsRequired();
        customer.HasIndex(c => c.Name);
    }
}
