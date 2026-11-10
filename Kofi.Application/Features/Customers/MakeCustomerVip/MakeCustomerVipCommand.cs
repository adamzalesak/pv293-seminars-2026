using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Customers.MakeCustomerVip;

public record MakeCustomerVipCommand : ICommand
{
    public Guid Id { get; init; }
}
