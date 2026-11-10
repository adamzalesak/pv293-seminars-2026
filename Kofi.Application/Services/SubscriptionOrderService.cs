using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Domain;
using Kofi.Domain.Orders;
using Kofi.Domain.Products;
using Kofi.Domain.Subscriptions;

namespace Kofi.Application.Services;

public class SubscriptionOrderService(
    ICustomerRepository customers,
    IProductRepository products,
    IOrderRepository orders,
    IStockService stock,
    IPricingService pricing,
    IPaymentGatewayClient payments) : ISubscriptionOrderService
{
    public async Task Create(Guid customerId, DateTime now, PreparedDelivery delivery)
    {
        var customer = await customers.GetById(customerId) ?? throw new NotFoundException("Customer not found");

        var reservations = new List<(Product Product, int Quantity)>();

        foreach (var group in delivery.Lines.GroupBy(l => l.ProductId))
        {
            var product = await products.GetById(group.Key) ?? throw new NotFoundException("Product not found");
            if (!product.IsActive)
            {
                throw new DomainRuleException("A delivery product is no longer available");
            }

            var quantity = group.Sum(l => l.Quantity.Value);
            if (product.StockQuantity < quantity)
            {
                throw new DomainRuleException("Not enough stock for the complete delivery");
            }

            reservations.Add((product, quantity));
        }

        var order = Order.Create(delivery.OrderId, customer.Id, customer.Name, customer.Email,
            customer.Street, customer.City, customer.Zip, now,
            delivery.Lines.Select(line => OrderItem.Create(line.ProductId, line.ProductName, line.Quantity, line.UnitPrice)));

        // A subscription is paid with the customer's saved card.
        if (customer.PaymentMethodToken == null)
        {
            throw new DomainRuleException("The customer has no saved card");
        }

        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var payment = await payments.Charge(
            Math.Round(itemsTotal + pricing.CalculateShipping(itemsTotal), 2), "EUR", order.Id.ToString(),
            customer.PaymentMethodToken, order.Id.ToString());
        if (payment.Status != PaymentStatus.Captured)
        {
            throw new DomainRuleException("The saved card was declined");
        }

        order.RecordPayment(payment.PaymentId);

        foreach (var reservation in reservations)
        {
            stock.Reserve(reservation.Product, reservation.Quantity);
        }

        orders.Add(order);
    }
}
