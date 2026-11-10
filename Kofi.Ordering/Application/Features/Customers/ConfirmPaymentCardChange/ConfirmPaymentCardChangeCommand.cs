using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Customers.ConfirmPaymentCardChange;

public record ConfirmPaymentCardChangeCommand : ICommand<ConfirmPaymentCardChangeResponse>
{
    public Guid SetupId { get; init; }
}
