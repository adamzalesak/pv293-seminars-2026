using Kofi.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kofi.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> customer)
    {
        customer.ToTable("Customers");
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
