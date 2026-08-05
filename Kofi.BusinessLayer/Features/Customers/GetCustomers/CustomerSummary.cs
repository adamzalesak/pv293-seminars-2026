namespace Kofi.BusinessLayer.Features.Customers.GetCustomers;

public record CustomerSummary
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required bool IsVip { get; init; }
}
