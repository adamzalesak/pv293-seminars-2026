namespace Kofi.Ordering.Domain.Orders;

public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = new();

    private Order()
    {
    }

    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = "";
    public Email CustomerEmail { get; private set; } = null!;
    public OrderStatus Status { get; private set; } = OrderStatus.New;
    public Guid? PaymentId { get; private set; }
    public ShippingAddress ShippingAddress { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public static Order Create(Guid id, Guid customerId, string name, Email email, ShippingAddress address, DateTime now,
        IEnumerable<OrderItem> items)
    {
        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            CustomerName = name,
            CustomerEmail = new Email(email.Value),
            ShippingAddress = new ShippingAddress(address.Street, address.City, address.Zip),
            CreatedAt = now
        };
        order._items.AddRange(items);

        if (order._items.Count == 0)
        {
            throw new DomainRuleException("An order needs at least one item.");
        }

        return order;
    }

    private void EnsureEditable()
    {
        if (Status != OrderStatus.New || PaymentId != null)
        {
            throw new DomainRuleException("This order can no longer change.");
        }
    }

    public void AddItem(OrderItem item)
    {
        EnsureEditable();
        _items.Add(item);
    }

    public OrderItem RemoveItem(Guid itemId)
    {
        EnsureEditable();
        var item = _items.SingleOrDefault(i => i.Id == itemId) ?? throw new DomainRuleException("Item not found.");

        if (_items.Count == 1)
        {
            throw new DomainRuleException("An order needs at least one item; cancel it instead.");
        }

        _items.Remove(item);
        return item;
    }

    public void ChangeAddress(ShippingAddress address)
    {
        if (Status is not (OrderStatus.New or OrderStatus.Paid))
        {
            throw new DomainRuleException("The address can only change before the order ships.");
        }

        ShippingAddress = new ShippingAddress(address.Street, address.City, address.Zip);
    }

    public void StartPayment(Guid paymentId)
    {
        if (Status != OrderStatus.New)
        {
            throw new DomainRuleException("Only new orders can be paid.");
        }

        PaymentId = paymentId;
    }

    public void RecordPayment(Guid paymentId)
    {
        if (Status != OrderStatus.New)
        {
            throw new DomainRuleException("Only new orders can be paid.");
        }

        PaymentId = paymentId;
        Status = OrderStatus.Paid;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
        {
            throw new DomainRuleException("Only paid orders can be shipped.");
        }

        Status = OrderStatus.Shipped;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Cancelled or OrderStatus.Shipped)
        {
            throw new DomainRuleException("This order cannot be cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }
}
