using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Application.Persistence;
using Kofi.Ordering.Application.Ports;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Services;
using Kofi.Ordering.Contracts;
using Kofi.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Application;

public class OrderingModule(
    IOrderingDbContext db,
    IOrderRepository orders,
    ICustomerRepository customers,
    IProductCatalog catalog,
    IOrderStock stock,
    IPaymentGatewayClient payments,
    IPricingService pricing) : IOrdering
{
    public async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, CancellationToken ct = default)
    {
        var customer = await customers.GetById(request.CustomerId) ?? throw new NotFoundException("Customer not found");

        foreach (var id in request.Lines.Select(l => l.ProductId).Distinct())
        {
            var product = await catalog.GetProduct(id, ct) ?? throw new NotFoundException("Product not found");
            if (!product.CanBeSold)
            {
                throw new DomainRuleException("A delivery product is no longer available");
            }
        }

        var reservations = request.Lines.Select(l => new StockLine(l.ProductId, l.Quantity)).ToList();
        await stock.CheckAvailable(reservations, ct);
        var order = Order.Create(Guid.NewGuid(), customer.Id, customer.Name, customer.Email, customer.ShippingAddress, DateTime.Now,
            request.Lines.Select(l =>
                OrderItem.Create(l.ProductId, l.ProductName, new Quantity(l.Quantity), new Price(l.UnitPrice, l.Currency))));

        if (customer.PaymentMethodToken == null)
        {
            throw new DomainRuleException("The customer has no saved card");
        }

        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var payment = await payments.Charge(Math.Round(itemsTotal + pricing.CalculateShipping(itemsTotal), 2), "EUR", order.Id.ToString(),
            customer.PaymentMethodToken, order.Id.ToString());
        if (payment.Status != PaymentStatus.Captured)
        {
            throw new DomainRuleException("The saved card was declined");
        }

        order.RecordPayment(payment.PaymentId);
        await stock.Reserve(reservations, ct);
        orders.Add(order);

        return new CreateOrderResponse(order.Id);
    }

    public Task<CustomerInfo?> GetCustomer(Guid id, CancellationToken ct = default) => db.Customers.AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new CustomerInfo(c.Id, c.Name, c.Email.Value, c.PaymentMethodToken != null, c.PaymentCardLabel))
        .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<Guid, string>> GetCustomerNames(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        await db.Customers.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, ct);

    public async Task SavePaymentCard(Guid customerId, string token, string? label, CancellationToken ct = default)
    {
        var customer = await customers.GetById(customerId) ?? throw new NotFoundException("Customer not found");

        customer.SavePaymentCard(token, label);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetOrderStatuses(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        await db.Orders.AsNoTracking().Where(o => ids.Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.Status.ToString(), ct);
}
