using Kofi.Common.Services;
using Kofi.Ordering.Application.Services;
using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Infrastructure.Services;

public class OrderEmailService(IEmailTransport transport, IPricingService pricing) : IOrderEmailService
{
    public async Task SendOrderConfirmation(Order order)
    {
        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var discount = pricing.CalculateDiscount(itemsTotal, order.Coupon?.DiscountPercent);
        var shipping = pricing.CalculateShipping(itemsTotal);
        var total = pricing.CalculateTotal(itemsTotal, order.Coupon?.DiscountPercent);

        var body = $"""
                    Hello {order.CustomerName},

                    thank you for your order {order.Id}.

                    Items:     {itemsTotal:N2} EUR{(discount > 0 ? $"\nDiscount: -{discount:N2} EUR ({order.Coupon!.Code})" : "")}
                    Shipping:  {shipping:N2} EUR
                    Total:     {total:N2} EUR

                    Kofi
                    """;

        await transport.Send(order.CustomerEmail.Value, $"Your KoFI order {order.Id}", body);
    }

    public async Task SendOrderCancelled(Order order)
    {
        await transport.Send(order.CustomerEmail.Value, $"Order {order.Id} cancelled",
            $"Your order {order.Id} was cancelled.");
    }

    public async Task SendShippingNotification(Order order)
    {
        await transport.Send(order.CustomerEmail.Value, $"Order {order.Id} shipped",
            $"Your order {order.Id} has been shipped.");
    }
}
