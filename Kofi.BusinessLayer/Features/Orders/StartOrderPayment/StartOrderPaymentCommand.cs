using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.StartOrderPayment;

public record StartOrderPaymentCommand : ICommand<StartOrderPaymentResponse>
{
    public Guid Id { get; init; }
    public string ReturnUrl { get; init; } = "";
}
