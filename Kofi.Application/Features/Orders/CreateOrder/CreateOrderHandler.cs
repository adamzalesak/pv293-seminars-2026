using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;
using Kofi.Domain;
using Kofi.Domain.Orders;
using Kofi.Domain.Shared;

namespace Kofi.Application.Features.Orders.CreateOrder;

public class CreateOrderHandler(
    IOrderRepository orders,
    IProductRepository products,
    ICustomerRepository customers,
    IStockService stock,
    IPricingService pricing,
    IEmailService email,
    ICurrentUser user) : ICommandHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var customer = await customers.GetById(user.CustomerId ?? Guid.Empty) ?? throw new NotFoundException("Customer not found");

        var lines = new List<OrderItem>();

        foreach (var requested in command.Items)
        {
            var product = await products.GetById(requested.ProductId) ?? throw new NotFoundException("Product not found");
            if (!product.IsActive)
            {
                throw new DomainRuleException("Product is no longer available");
            }

            stock.Reserve(product, requested.Quantity);
            lines.Add(OrderItem.Create(product.Id, product.Name, new Quantity(requested.Quantity), product.Price));
        }

        var order = Order.Create(Guid.NewGuid(), customer.Id, command.CustomerName, customer.Email,
            command.Street, command.City, command.Zip, DateTime.Now, lines);
        orders.Add(order);
        await email.SendOrderConfirmation(order);
        var itemsTotal = pricing.CalculateItemsTotal(order.Items);

        return new CreateOrderResponse
            { Id = order.Id, Status = order.Status.ToString(), Total = Math.Round(itemsTotal + pricing.CalculateShipping(itemsTotal), 2) };
    }
}
