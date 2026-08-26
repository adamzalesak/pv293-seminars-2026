using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Customers.StartPaymentCardChange;

public record StartPaymentCardChangeCommand : ICommand<StartPaymentCardChangeResponse>
{
    public string ReturnUrl { get; init; } = "";
}
