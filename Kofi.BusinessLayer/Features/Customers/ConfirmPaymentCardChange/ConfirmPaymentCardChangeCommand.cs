using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Customers.ConfirmPaymentCardChange;

public record ConfirmPaymentCardChangeCommand : ICommand<ConfirmPaymentCardChangeResponse>
{
    public Guid SetupId { get; init; }
}
