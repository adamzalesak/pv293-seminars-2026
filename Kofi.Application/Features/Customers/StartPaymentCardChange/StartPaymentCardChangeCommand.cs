using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Customers.StartPaymentCardChange;

public record StartPaymentCardChangeCommand : ICommand<StartPaymentCardChangeResponse>
{
    public string ReturnUrl { get; init; } = "";
}
