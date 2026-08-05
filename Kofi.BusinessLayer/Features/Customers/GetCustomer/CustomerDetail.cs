namespace Kofi.BusinessLayer.Features.Customers.GetCustomer;

public record CustomerDetail
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Street { get; init; }
    public required string City { get; init; }
    public required string Zip { get; init; }
    public required DateTime RegisteredAt { get; init; }
    public required bool IsVip { get; init; }
}
