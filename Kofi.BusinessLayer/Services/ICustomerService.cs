using Kofi.BusinessLayer.Dtos;

namespace Kofi.BusinessLayer.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetCustomers();
    Task<CustomerDto?> GetCustomer(Guid id);
    Task<CustomerDto> RegisterCustomer(CustomerDto request);
    Task MakeVip(Guid id);
}
