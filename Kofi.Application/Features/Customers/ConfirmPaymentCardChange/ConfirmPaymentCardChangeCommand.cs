using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Customers.ConfirmPaymentCardChange;

public record ConfirmPaymentCardChangeCommand : ICommand<ConfirmPaymentCardChangeResponse>
{
    public Guid SetupId { get; init; }
}
