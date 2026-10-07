using Kofi.Catalog.Contracts;
using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Marketing.Contracts;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Services;
using Kofi.Ordering.Domain.Orders;
using Kofi.Warehouse.Contracts;

namespace Kofi.Ordering.Application.Features.Orders.CreateOrder;

public class CreateOrderHandler(
    IOrderRepository orders,
    ICatalog products,
    ICustomerRepository customers,
    IWarehouse stock,
    IPricingService pricing,
    IOrderEmailService email,
    IMarketing marketing,
    ICurrentUser user) : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var customer = await customers.GetById(user.CustomerId ?? Guid.Empty) ?? throw new NotFoundException("Customer not found");

        AppliedCoupon? coupon = null;
        if (!string.IsNullOrWhiteSpace(command.CouponCode))
        {
            var discount = await marketing.ValidateCoupon(command.CouponCode, customer.Id, ct);
            coupon = new AppliedCoupon(discount.Code, discount.DiscountPercent);
        }

        var lines = new List<OrderItem>();

        foreach (var requested in command.Items)
        {
            var product = await products.GetProduct(requested.ProductId, ct) ?? throw new NotFoundException("Product not found");
            if (!product.CanBeSold)
            {
                throw new DomainRuleException("Product is no longer available");
            }

            await stock.Reserve([new StockLine(product.Id, requested.Quantity)], ct);
            lines.Add(OrderItem.Create(product.Id, product.Name, new Quantity(requested.Quantity),
                new Price(product.Price, product.Currency)));
        }

        var order = Order.Create(Guid.NewGuid(), customer.Id, command.CustomerName, customer.Email,
            new ShippingAddress(command.Street, command.City, command.Zip), DateTime.Now, lines, coupon);
        orders.Add(order);
        await email.SendOrderConfirmation(order);
        var total = pricing.CalculateTotal(pricing.CalculateItemsTotal(order.Items), order.Coupon?.DiscountPercent);

        return new CreateOrderResponse { Id = order.Id, Status = order.Status.ToString(), Total = total };
    }
}
