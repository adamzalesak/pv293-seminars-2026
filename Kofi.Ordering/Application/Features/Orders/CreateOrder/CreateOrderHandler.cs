using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Application.Ports;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Services;
using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Application.Features.Orders.CreateOrder;

public class CreateOrderHandler(
    IOrderRepository orders,
    IProductCatalog products,
    ICustomerRepository customers,
    IOrderStock stock,
    IPricingService pricing,
    IOrderEmailService email,
    ICurrentUser user) : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var customer = await customers.GetById(user.CustomerId ?? Guid.Empty) ?? throw new NotFoundException("Customer not found");

        var lines = new List<OrderItem>();

        foreach (var requested in command.Items)
        {
            var product = await products.GetProduct(requested.ProductId) ?? throw new NotFoundException("Product not found");
            if (!product.CanBeSold)
            {
                throw new DomainRuleException("Product is no longer available");
            }

            await stock.Reserve([new StockLine(product.Id, requested.Quantity)], ct);
            lines.Add(OrderItem.Create(product.Id, product.Name, new Quantity(requested.Quantity),
                new Price(product.Price, product.Currency)));
        }

        var order = Order.Create(Guid.NewGuid(), customer.Id, command.CustomerName, customer.Email,
            new ShippingAddress(command.Street, command.City, command.Zip), DateTime.Now, lines);
        orders.Add(order);
        await email.SendOrderConfirmation(order);
        var itemsTotal = pricing.CalculateItemsTotal(order.Items);

        return new CreateOrderResponse
            { Id = order.Id, Status = order.Status.ToString(), Total = Math.Round(itemsTotal + pricing.CalculateShipping(itemsTotal), 2) };
    }
}
