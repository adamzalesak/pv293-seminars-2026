using Kofi.BusinessLayer.Dtos;
using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Mappers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Street = customer.Street,
            City = customer.City,
            Zip = customer.Zip,
            RegisteredAt = customer.RegisteredAt,
            IsVip = customer.IsVip,
        };
    }
}
