using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Data;
using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public async Task<Customer?> GetById(Guid id)
    {
        return await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Customer>> GetAll()
    {
        return await db.Customers.ToListAsync();
    }

    public async Task Add(Customer customer)
    {
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
    }

    public async Task Update(Customer customer)
    {
        db.Customers.Update(customer);
        await db.SaveChangesAsync();
    }
}
