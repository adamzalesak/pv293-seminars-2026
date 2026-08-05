using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetById(Guid id);
    Task<bool> EmailTaken(string email);
    Task<List<Customer>> GetAll();
    void Add(Customer customer);
    void Update(Customer customer);
}
