using Kofi.Ordering.Domain.Customers;
using Kofi.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Application.Persistence;

public interface IOrderingDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<Customer> Customers { get; }
}
