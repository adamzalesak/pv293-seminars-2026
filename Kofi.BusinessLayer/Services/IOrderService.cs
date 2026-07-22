using Kofi.BusinessLayer.Dtos;

namespace Kofi.BusinessLayer.Services;

public interface IOrderService
{
    Task<List<OrderDto>> GetOrders(
        Guid? customerId = null,
        bool includeItems = false,
        bool onlyPaid = false,
        DateTime? from = null,
        string? sortBy = null);
    Task<OrderDto?> GetOrderDetail(Guid id, Guid? customerId = null);
    Task<OrderDto> CreateOrder(Guid customerId, OrderDto request);
    Task CancelOrder(Guid id);
    Task<(Guid PaymentId, string RedirectUrl)> StartPayment(
        Guid id, Guid customerId, string returnUrl);
    Task<PaymentOutcome> ConfirmPayment(Guid id, Guid paymentId);
    Task MarkAsShipped(Guid id);
    Task UpdateShippingAddress(Guid id, string street, string city, string zip);
    Task AddItemToOrder(Guid orderId, Guid productId, int quantity);
    Task RemoveItemFromOrder(Guid orderId, Guid orderItemId);
    Task<int> GetOrderCountByStatus(string status);
    Task ResendConfirmationEmail(Guid id);
    Task<string> ExportOrdersToCsv(DateTime? from);
}
