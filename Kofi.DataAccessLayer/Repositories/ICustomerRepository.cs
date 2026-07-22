using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetById(Guid id);
    Task<List<Customer>> GetAll();
    Task Add(Customer customer);
    Task Update(Customer customer);
}
