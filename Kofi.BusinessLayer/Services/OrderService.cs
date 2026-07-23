using System.Text;
using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Data;
using Kofi.BusinessLayer.Dtos;
using Kofi.BusinessLayer.Mappers;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Services;

public class OrderService(
    AppDbContext db,
    IOrderRepository orders,
    IProductRepository products,
    ICustomerRepository customers,
    IStockService stock,
    IPricingService pricing,
    IEmailService email,
    IAuditService audit,
    IPaymentGatewayClient payments) : IOrderService
{
    // Serves the admin and customer order lists and the CSV export.
    public async Task<List<OrderDto>> GetOrders(
        Guid? customerId = null,
        bool includeItems = false,
        bool onlyPaid = false,
        DateTime? from = null,
        string? sortBy = null)
    {
        var query = orders.Query();

        if (customerId != null)
        {
            query = query.Where(o => o.CustomerId == customerId);
        }

        if (onlyPaid)
        {
            query = query.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped);
        }

        if (from != null)
        {
            query = query.Where(o => o.CreatedAt >= from.Value);
        }

        if (includeItems)
        {
            query = query.Include(o => o.Items);
        }

        if (sortBy == "date")
        {
            query = query.OrderByDescending(o => o.CreatedAt);
        }
        else if (sortBy == "total")
        {
            query = query.OrderByDescending(o => o.Items.Sum(i => i.UnitPrice * i.Quantity));
        }
        else if (sortBy == "customer")
        {
            query = query.OrderBy(o => o.CustomerName);
        }
        else
        {
            query = query.OrderByDescending(o => o.CreatedAt);
        }

        // The total is summed in the database, so a listing does not drag
        // every order item across the wire to add it up.
        var rows = await query
            .Select(o => new
            {
                Order = o,
                ItemsTotal = o.Items.Sum(i => i.UnitPrice * i.Quantity),
            })
            .ToListAsync();

        var result = new List<OrderDto>();
        foreach (var row in rows)
        {
            var dto = row.Order.ToDto();
            dto.Total = row.ItemsTotal < 50
                ? Math.Round(row.ItemsTotal + 4.99m, 2)
                : Math.Round(row.ItemsTotal, 2);
            result.Add(dto);
        }
        return result;
    }

    // A customer sees only their own order; anyone else's does not exist.
    public async Task<OrderDto?> GetOrderDetail(Guid id, Guid? customerId = null)
    {
        var order = await orders.GetByIdWithItems(id);
        if (order == null || (customerId != null && order.CustomerId != customerId))
        {
            return null;
        }

        decimal itemsTotal = 0;
        foreach (var item in order.Items)
        {
            itemsTotal = itemsTotal + item.UnitPrice * item.Quantity;
        }

        decimal shipping = 0;
        if (itemsTotal < 50)
        {
            shipping = 4.99m;
        }

        var dto = order.ToDto();
        dto.ItemsTotal = Math.Round(itemsTotal, 2);
        dto.Shipping = shipping;
        dto.Total = Math.Round(itemsTotal + shipping, 2);
        return dto;
    }

    public async Task<OrderDto> CreateOrder(Guid customerId, OrderDto request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new Exception("Order must contain at least one item");
        }
        if (request.Items.Any(i => i.Quantity < 1))
        {
            throw new Exception("Quantity must be at least 1");
        }

        var customer = await customers.GetById(customerId);
        if (customer == null)
        {
            throw new Exception("Customer not found");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CustomerName = request.CustomerName,
            CustomerEmail = customer.Email,
            Street = request.Street,
            City = request.City,
            Zip = request.Zip,
            Status = OrderStatus.New,
            CreatedAt = DateTime.Now,
        };

        decimal itemsTotal = 0;

        foreach (var requested in request.Items)
        {
            var product = await products.GetById(requested.ProductId);
            if (product == null)
            {
                throw new Exception("Product not found: " + requested.ProductId);
            }
            if (!product.IsActive)
            {
                throw new Exception("Product is no longer available: " + product.Name);
            }
            if (product.StockQuantity < requested.Quantity)
            {
                throw new Exception("Not enough stock for " + product.Name);
            }

            await stock.Reserve(product, requested.Quantity);

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = requested.Quantity,
            });

            itemsTotal = itemsTotal + product.Price * requested.Quantity;
        }

        var total = itemsTotal < 50 ? itemsTotal + 4.99m : itemsTotal;

        orders.Add(order);

        await email.SendOrderConfirmation(order);

        var dto = order.ToDto();
        dto.Total = Math.Round(total, 2);
        return dto;
    }

    public async Task CancelOrder(Guid id)
    {
        var order = await orders.GetByIdWithItems(id);
        if (order == null)
        {
            throw new Exception("Order not found");
        }
        if (order.Status == OrderStatus.Cancelled)
        {
            throw new Exception("The order is already cancelled");
        }
        if (order.Status == OrderStatus.Shipped)
        {
            throw new Exception("Shipped orders cannot be cancelled");
        }

        // An open payment is stopped, a finished one is refunded.
        if (order.PaymentId != null)
        {
            await payments.CancelOrRefund(order.PaymentId.Value);
        }

        using var transaction = await db.Database.BeginTransactionAsync();

        order.Status = OrderStatus.Cancelled;
        await orders.Update(order);

        foreach (var item in order.Items)
        {
            await stock.Release(item.ProductId, item.Quantity);
        }

        await transaction.CommitAsync();

        await email.SendOrderCancelled(order);
        audit.Record("OrderCancelled", order.Id);
    }

    // The customer pays on the provider's page. We only learn the result
    // when we ask the provider ourselves, in ConfirmPayment.
    public async Task<(Guid PaymentId, string RedirectUrl)> StartPayment(
        Guid id, Guid customerId, string returnUrl)
    {
        var order = await orders.GetByIdWithItems(id);
        if (order == null || order.CustomerId != customerId)
        {
            throw new Exception("Order not found");
        }
        if (order.Status != OrderStatus.New)
        {
            throw new Exception("Only new orders can be paid");
        }

        // While the last attempt is still open, the customer gets the same
        // payment page back. A new attempt only after a decline.
        if (order.PaymentId != null)
        {
            var previous = await payments.GetPayment(order.PaymentId.Value);
            if (previous.Status == PaymentStatus.Created)
            {
                return (order.PaymentId.Value, previous.RedirectUrl);
            }
            if (previous.Status == PaymentStatus.Captured)
            {
                // Paid, but the customer never came back from the payment
                // page: send them back to it now, and it records the payment.
                return (order.PaymentId.Value, $"{returnUrl}?paymentId={order.PaymentId}");
            }
        }
        var attempt = order.PaymentId == null ? order.Id.ToString() : $"{order.Id}/{order.PaymentId}";

        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var total = Math.Round(itemsTotal + pricing.CalculateShipping(itemsTotal), 2);
        var payment = await payments.CreatePayment(total, "EUR", order.Id.ToString(), returnUrl, attempt);

        order.PaymentId = payment.PaymentId;
        await orders.Update(order);
        return payment;
    }

    // Returns what the customer is told: Paid, Pending, Declined or Cancelled.
    public async Task<PaymentOutcome> ConfirmPayment(Guid id, Guid paymentId)
    {
        var order = await orders.GetById(id);
        if (order == null || order.PaymentId != paymentId)
        {
            throw new Exception("Unknown payment");
        }

        // Never trust the redirect: ask the provider what really happened.
        var status = await payments.GetStatus(paymentId);
        if (order.Status == OrderStatus.Cancelled)
        {
            // The order was cancelled meanwhile. Money that was taken goes back.
            if (status == PaymentStatus.Captured)
            {
                await payments.CancelOrRefund(paymentId);
            }
            return PaymentOutcome.Cancelled;
        }
        if (status == PaymentStatus.Captured && order.Status == OrderStatus.New)
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            order.Status = OrderStatus.Paid;
            await orders.Update(order);

            await transaction.CommitAsync();

            audit.Record("OrderPaid", order.Id);
        }

        return status switch
        {
            PaymentStatus.Captured => PaymentOutcome.Paid,
            PaymentStatus.Created => PaymentOutcome.Pending,
            PaymentStatus.Declined => PaymentOutcome.Declined,
            _ => PaymentOutcome.Cancelled,
        };
    }

    public async Task MarkAsShipped(Guid id)
    {
        var order = await orders.GetById(id);
        if (order == null)
        {
            throw new Exception("Order not found");
        }
        if (order.Status != OrderStatus.Paid)
        {
            throw new Exception("Only paid orders can be shipped");
        }

        using var transaction = await db.Database.BeginTransactionAsync();

        order.Status = OrderStatus.Shipped;
        await orders.Update(order);

        await transaction.CommitAsync();

        await email.SendShippingNotification(order);
    }

    public async Task UpdateShippingAddress(Guid id, string street, string city, string zip)
    {
        var order = await orders.GetById(id);
        if (order == null)
        {
            throw new Exception("Order not found");
        }
        if (order.Status != OrderStatus.New && order.Status != OrderStatus.Paid)
        {
            throw new Exception("The address can only change before the order ships");
        }
        if (string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(zip))
        {
            throw new Exception("Street, city and postcode are required");
        }

        order.Street = street;
        order.City = city;
        order.Zip = zip;

        await orders.Update(order);
    }

    public async Task AddItemToOrder(Guid orderId, Guid productId, int quantity)
    {
        var order = await orders.GetByIdWithItems(orderId);
        if (order == null)
        {
            throw new Exception("Order not found");
        }
        // Only a new order whose payment has not started can change.
        if (order.Status != OrderStatus.New || order.PaymentId != null)
        {
            throw new Exception("This order can no longer change");
        }
        if (quantity < 1)
        {
            throw new Exception("Quantity must be at least 1");
        }

        var product = await products.GetById(productId);
        if (product == null)
        {
            throw new Exception("Product not found");
        }
        if (!product.IsActive)
        {
            throw new Exception("Product is no longer available: " + product.Name);
        }
        if (product.StockQuantity < quantity)
        {
            throw new Exception("Not enough stock for " + product.Name);
        }

        using var transaction = await db.Database.BeginTransactionAsync();

        await stock.Reserve(product, quantity);

        order.Items.Add(new OrderItem
        {
            OrderId = order.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = quantity,
        });

        await orders.Update(order);

        await transaction.CommitAsync();
    }

    public async Task RemoveItemFromOrder(Guid orderId, Guid orderItemId)
    {
        var order = await orders.GetByIdWithItems(orderId);
        if (order == null)
        {
            throw new Exception("Order not found");
        }
        // Only a new order whose payment has not started can change.
        if (order.Status != OrderStatus.New || order.PaymentId != null)
        {
            throw new Exception("This order can no longer change");
        }

        var item = order.Items.FirstOrDefault(i => i.Id == orderItemId);
        if (item == null)
        {
            throw new Exception("Item not found");
        }
        if (order.Items.Count == 1)
        {
            throw new Exception("An order needs at least one item; cancel it instead");
        }

        using var transaction = await db.Database.BeginTransactionAsync();

        order.Items.Remove(item);
        await stock.Release(item.ProductId, item.Quantity);
        await orders.Update(order);

        await transaction.CommitAsync();
    }

    public async Task<int> GetOrderCountByStatus(string status)
    {
        // The status comes from the URL as text.
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
        {
            return 0;
        }

        return await orders.CountByStatus(parsed);
    }

    public async Task ResendConfirmationEmail(Guid id)
    {
        var order = await orders.GetByIdWithItems(id);
        if (order == null)
        {
            throw new Exception("Order not found");
        }
        await email.SendOrderConfirmation(order);
    }

    public async Task<string> ExportOrdersToCsv(DateTime? from)
    {
        var list = await GetOrders(from: from, sortBy: "date");

        var csv = new StringBuilder();
        csv.AppendLine("Id;Customer;Status;CreatedAt;Total");
        foreach (var order in list)
        {
            csv.AppendLine($"{order.Id};{order.CustomerName};{order.Status};{order.CreatedAt};{order.Total}");
        }
        return csv.ToString();
    }
}
