using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Services;

public class SubscriptionOrderService(
    ICustomerRepository customers, IProductRepository products, IOrderRepository orders, IStockService stock,
    IPricingService pricing, IPaymentGatewayClient payments) : ISubscriptionOrderService
{
    public async Task Create(Guid customerId, Guid orderId, DateTime now, IReadOnlyList<SubscriptionOrderLine> lines)
    {
        var customer = await customers.GetById(customerId) ?? throw new DomainException("Customer not found");
        var reservations = new List<(Product Product, int Quantity)>();
        foreach (var group in lines.GroupBy(l => l.ProductId))
        {
            var product = await products.GetById(group.Key) ?? throw new DomainException("Product not found");
            if (!product.IsActive) throw new DomainException("A delivery product is no longer available");
            var quantity = group.Sum(l => l.Quantity);
            if (product.StockQuantity < quantity) throw new DomainException("Not enough stock for the complete delivery");
            reservations.Add((product, quantity));
        }
        var order = new Order
        {
            Id = orderId, CustomerId = customer.Id, CustomerName = customer.Name,
            CustomerEmail = customer.Email, Street = customer.Street, City = customer.City,
            Zip = customer.Zip, CreatedAt = now,
        };
        foreach (var line in lines)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(), OrderId = order.Id, ProductId = line.ProductId,
                ProductName = line.ProductName, Quantity = line.Quantity, UnitPrice = line.UnitPrice,
            });
        }
        // A subscription is paid with the customer's saved card.
        if (customer.PaymentMethodToken == null) throw new DomainException("The customer has no saved card");
        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var payment = await payments.Charge(
            Math.Round(itemsTotal + pricing.CalculateShipping(itemsTotal), 2), "EUR", order.Id.ToString(),
            customer.PaymentMethodToken, order.Id.ToString());
        if (payment.Status != PaymentStatus.Captured) throw new DomainException("The saved card was declined");
        order.Status = OrderStatus.Paid;
        order.PaymentId = payment.PaymentId;
        foreach (var reservation in reservations) stock.Reserve(reservation.Product, reservation.Quantity);
        orders.Add(order);
    }
}
