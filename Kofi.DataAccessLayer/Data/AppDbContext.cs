using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionDelivery> SubscriptionDeliveries => Set<SubscriptionDelivery>();

    // The shop works in local time, so timestamps are stored without a time
    // zone.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Statuses are stored as text, readable in the table.
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        // Two decimal places, the same as every price we store.
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();
        modelBuilder.Entity<Subscription>(subscription =>
        {
            subscription.Property(s => s.Id).ValueGeneratedNever();
            subscription.Property(s => s.Status).HasConversion<string>();
            subscription.HasOne(s => s.Customer).WithMany(c => c.Subscriptions)
                .HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
            subscription.HasOne(s => s.Product).WithMany()
                .HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
            subscription.HasMany(s => s.Deliveries).WithOne(d => d.Subscription)
                .HasForeignKey(d => d.SubscriptionId);
        });
        modelBuilder.Entity<SubscriptionDelivery>(delivery =>
        {
            // IDs are assigned by the application, including rows appended to
            // an already tracked collection.
            delivery.Property(d => d.Id).ValueGeneratedNever();
            delivery.Property(d => d.Status).HasConversion<string>();
            delivery.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
            delivery.HasOne(d => d.Order).WithMany()
                .HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<SubscriptionDeliveryExtra>(extra =>
        {
            extra.HasKey(e => new { e.DeliveryId, e.ProductId });
            extra.HasOne(e => e.Delivery).WithMany(d => d.Extras).HasForeignKey(e => e.DeliveryId);
            extra.Property(e => e.UnitPrice).HasPrecision(18, 2);
        });
    }
}
