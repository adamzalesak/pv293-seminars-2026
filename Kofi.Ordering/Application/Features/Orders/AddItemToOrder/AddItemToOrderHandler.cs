using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Ports;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Application.Features.Orders.AddItemToOrder;

public class AddItemToOrderHandler(IOrderRepository orders, IProductCatalog products, IOrderStock stock)
    : ICommandHandler<AddItemToOrderCommand>
{
    public async Task Handle(AddItemToOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.OrderId) ?? throw new NotFoundException("Order not found");

        var product = await products.GetProduct(command.ProductId) ?? throw new NotFoundException("Product not found");
        if (!product.CanBeSold)
        {
            throw new DomainRuleException("Product is no longer available");
        }

        order.AddItem(
            OrderItem.Create(product.Id, product.Name, new Quantity(command.Quantity), new Price(product.Price, product.Currency)));
        await stock.Reserve([new StockLine(product.Id, command.Quantity)], ct);
    }
}
