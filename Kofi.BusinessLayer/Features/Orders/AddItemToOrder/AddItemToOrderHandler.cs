using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.AddItemToOrder;

public class AddItemToOrderHandler(
    IOrderRepository orders,
    IProductRepository products,
    IStockService stock) : ICommandHandler<AddItemToOrderCommand>
{
    public async Task Handle(AddItemToOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdWithItems(command.OrderId)
            ?? throw new DomainException("Order not found");
        // Only a new order whose payment has not started can change.
        if (order.Status != OrderStatus.New || order.PaymentId != null)
        {
            throw new DomainException("This order can no longer change");
        }

        var product = await products.GetById(command.ProductId)
            ?? throw new DomainException("Product not found");
        if (!product.IsActive)
        {
            throw new DomainException("Product is no longer available: " + product.Name);
        }
        if (product.StockQuantity < command.Quantity)
        {
            throw new DomainException("Not enough stock for " + product.Name);
        }

        stock.Reserve(product, command.Quantity);

        order.Items.Add(new OrderItem
        {
            OrderId = order.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = command.Quantity,
        });

        orders.Update(order);
    }
}
