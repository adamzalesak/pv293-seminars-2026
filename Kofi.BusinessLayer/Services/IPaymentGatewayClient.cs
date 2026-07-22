namespace Kofi.BusinessLayer.Services;

public interface IPaymentGatewayClient
{
    Task<(Guid PaymentId, string RedirectUrl)> CreatePayment(
        decimal amount, string currency, string reference, string returnUrl, string idempotencyKey);
    Task<PaymentStatus> CancelOrRefund(Guid paymentId);
    Task<PaymentStatus> GetStatus(Guid paymentId);
    Task<(PaymentStatus Status, string RedirectUrl)> GetPayment(Guid paymentId);
}
