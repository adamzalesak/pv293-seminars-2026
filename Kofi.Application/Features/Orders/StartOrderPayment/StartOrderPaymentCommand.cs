using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.StartOrderPayment;

public record StartOrderPaymentCommand : ICommand<StartOrderPaymentResponse>
{
    public Guid Id { get; init; }
    public string ReturnUrl { get; init; } = "";
}
