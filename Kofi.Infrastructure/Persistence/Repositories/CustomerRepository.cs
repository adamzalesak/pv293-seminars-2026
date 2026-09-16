using Kofi.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Infrastructure.Persistence.Repositories;

using Kofi.Application.Repositories;

public class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public async Task<Customer?> GetById(Guid id)
    {
        return await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> EmailTaken(string email)
    {
        return await db.Customers.AnyAsync(c => c.Email.Value == email);
    }

    public void Add(Customer customer)
    {
        db.Customers.Add(customer);
    }
}
