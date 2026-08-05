using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.CreateOrder;

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
        // The customer placing the order is the one signed in.
        var customer = await customers.GetById(user.CustomerId ?? Guid.Empty)
            ?? throw new DomainException("Only a signed-in customer can place an order");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CustomerName = command.CustomerName,
            CustomerEmail = customer.Email,
            Street = command.Street,
            City = command.City,
            Zip = command.Zip,
            Status = OrderStatus.New,
            CreatedAt = DateTime.Now,
        };

        foreach (var requested in command.Items)
        {
            var product = await products.GetById(requested.ProductId);
            if (product == null)
            {
                throw new DomainException("Product not found: " + requested.ProductId);
            }
            if (!product.IsActive)
            {
                throw new DomainException("Product is no longer available: " + product.Name);
            }
            if (product.StockQuantity < requested.Quantity)
            {
                throw new DomainException("Not enough stock for " + product.Name);
            }

            stock.Reserve(product, requested.Quantity);

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = requested.Quantity,
            });
        }

        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var total = itemsTotal + pricing.CalculateShipping(itemsTotal);

        orders.Add(order);

        await email.SendOrderConfirmation(order);

        return new CreateOrderResponse
        {
            Id = order.Id,
            Status = order.Status.ToString(),
            Total = Math.Round(total, 2),
        };
    }
}
