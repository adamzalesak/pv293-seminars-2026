namespace Kofi.Common.Services;

public interface IPaymentGatewayClient
{
    Task<(Guid PaymentId, string RedirectUrl)> CreatePayment(
        decimal amount, string currency, string reference, string returnUrl, string idempotencyKey);

    Task<(Guid PaymentId, PaymentStatus Status)> Charge(
        decimal amount, string currency, string reference, string paymentMethodToken, string idempotencyKey);

    Task<PaymentStatus> CancelOrRefund(Guid paymentId);
    Task<PaymentStatus> GetStatus(Guid paymentId);
    Task<(PaymentStatus Status, string RedirectUrl)> GetPayment(Guid paymentId);
    Task<(Guid SetupId, string RedirectUrl)> CreateCardSetup(string reference, string returnUrl, string idempotencyKey);
    Task<(CardSetupStatus Status, string Reference, string? PaymentMethodToken, string? CardLabel)> GetCardSetup(Guid setupId);
}
