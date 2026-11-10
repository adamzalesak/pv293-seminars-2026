using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;
using Kofi.Domain;
using Kofi.Domain.Orders;
using Kofi.Domain.Shared;

namespace Kofi.Application.Features.Orders.AddItemToOrder;

public class AddItemToOrderHandler(IOrderRepository orders, IProductRepository products, IStockService stock)
    : ICommandHandler<AddItemToOrderCommand>
{
    public async Task Handle(AddItemToOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.OrderId) ?? throw new NotFoundException("Order not found");

        var product = await products.GetById(command.ProductId) ?? throw new NotFoundException("Product not found");
        if (!product.IsActive)
        {
            throw new DomainRuleException("Product is no longer available");
        }

        order.AddItem(OrderItem.Create(product.Id, product.Name, new Quantity(command.Quantity), product.Price));
        stock.Reserve(product, command.Quantity);
    }
}
