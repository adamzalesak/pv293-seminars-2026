using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Customers.RegisterCustomer;

public record RegisterCustomerCommand : ICommand<RegisterCustomerResponse>
{
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string Street { get; init; } = "";
    public string City { get; init; } = "";
    public string Zip { get; init; } = "";
}
