using Kofi.BusinessLayer.Dtos;
using Kofi.BusinessLayer.Mappers;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Services;

public class CustomerService(ICustomerRepository customers) : ICustomerService
{
    public async Task<List<CustomerDto>> GetCustomers()
    {
        var list = await customers.GetAll();
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<CustomerDto?> GetCustomer(Guid id)
    {
        var customer = await customers.GetById(id);
        return customer?.ToDto();
    }

    public async Task<CustomerDto> RegisterCustomer(CustomerDto request)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Street = request.Street,
            City = request.City,
            Zip = request.Zip,
            RegisteredAt = DateTime.Now,
            IsVip = false,
        };

        await customers.Add(customer);

        return customer.ToDto();
    }

    public async Task MakeVip(Guid id)
    {
        var customer = await customers.GetById(id);
        if (customer == null)
        {
            throw new Exception("Customer not found");
        }

        customer.IsVip = true;
        await customers.Update(customer);
    }
}
