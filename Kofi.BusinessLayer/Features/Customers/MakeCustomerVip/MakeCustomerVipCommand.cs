using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Customers.MakeCustomerVip;

public record MakeCustomerVipCommand : ICommand
{
    public Guid Id { get; init; }
}
