using Kofi.Common.Persistence;
using Kofi.Ordering.Application.Persistence;
using Kofi.Ordering.Domain.Customers;
using Kofi.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Infrastructure.Persistence;

public class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : AggregateDbContext(options), IOrderingDbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
}
