using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Customers.StartPaymentCardChange;

public record StartPaymentCardChangeCommand : ICommand<StartPaymentCardChangeResponse>
{
    public string ReturnUrl { get; init; } = "";
}
