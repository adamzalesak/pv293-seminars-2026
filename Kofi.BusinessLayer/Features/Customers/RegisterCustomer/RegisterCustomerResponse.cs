namespace Kofi.BusinessLayer.Features.Customers.RegisterCustomer;

public record RegisterCustomerResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
}
