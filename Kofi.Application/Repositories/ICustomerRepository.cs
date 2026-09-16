using Kofi.Domain.Customers;

namespace Kofi.Application.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetById(Guid id);
    Task<bool> EmailTaken(string email);
    void Add(Customer customer);
}
