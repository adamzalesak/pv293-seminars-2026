using Kofi.Domain.Orders;
using Kofi.Domain.Shared;
using Xunit;

namespace Kofi.Domain.Tests.Orders;

public class OrderRulesTests
{
    private static Order Create() => Order.Create(Guid.NewGuid(), Guid.NewGuid(), "Petra",
        new Email("petra@example.com"), "Kounicova 12", "Brno", "60200", DateTime.Today,
        [OrderItem.Create(Guid.NewGuid(), "Coffee", new Quantity(1), new Price(12, "EUR"))]);

    [Fact]
    public void Payment_started_freezes_items()
    {
        var order = Create();
        order.StartPayment(Guid.NewGuid());
        Assert.Throws<DomainRuleException>(() => order.AddItem(
            OrderItem.Create(Guid.NewGuid(), "Tea", new Quantity(1), new Price(4, "EUR"))));
    }

    [Fact]
    public void The_last_item_requires_cancelling_the_order()
    {
        var order = Create();
        Assert.Throws<DomainRuleException>(() => order.RemoveItem(order.Items.Single().Id));
    }

    [Fact]
    public void Shipped_orders_keep_their_address_and_cannot_be_cancelled()
    {
        var order = Create();
        order.RecordPayment(Guid.NewGuid());
        order.Ship();
        Assert.Throws<DomainRuleException>(() => order.ChangeAddress("Veveří 45", "Brno", "60200"));
        Assert.Throws<DomainRuleException>(order.Cancel);
    }
}
