using Kofi.Ordering.Domain.Customers;

namespace Kofi.Ordering.Application.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetById(Guid id);
    Task<bool> EmailTaken(string email);
    void Add(Customer customer);
}
