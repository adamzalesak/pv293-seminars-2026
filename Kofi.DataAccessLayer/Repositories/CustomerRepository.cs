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

    public async Task<bool> EmailTaken(string email)
    {
        return await db.Customers.AnyAsync(c => c.Email == email);
    }

    public async Task<List<Customer>> GetAll()
    {
        return await db.Customers.ToListAsync();
    }

    public void Add(Customer customer)
    {
        db.Customers.Add(customer);
    }

    public void Update(Customer customer)
    {
        db.Customers.Update(customer);
    }
}
