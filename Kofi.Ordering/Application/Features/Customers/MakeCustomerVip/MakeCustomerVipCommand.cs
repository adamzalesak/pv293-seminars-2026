using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Customers.MakeCustomerVip;

public record MakeCustomerVipCommand : ICommand
{
    public Guid Id { get; init; }
}
