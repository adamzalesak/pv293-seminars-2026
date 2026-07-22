using System.Text;
using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Data;
using Kofi.BusinessLayer.Dtos;
using Kofi.BusinessLayer.Mappers;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Services;

// Everything about orders. Started small.
public class OrderService(
    AppDbContext db,
    OrderRepository orders,
    ProductRepository products,
    CustomerRepository customers,
    StockService stock,
    PricingService pricing,
    EmailService email,
    AuditService audit,
    PaymentGatewayClient payments)
{
    // Used by the admin list, the customer list, the invoicing export,
    // the dashboard and the mobile app. Please don't change the defaults.
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
            var customer = await customers.GetById(customerId.Value);
            if (customer == null)
            {
                return new List<OrderDto>();
            }
            query = query.Where(o => o.CustomerEmail == customer.Email);
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
        // every order item across the wire to add it up. Free delivery
        // from 50.
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

    public async Task<Order?> GetOrderById(Guid id)
    {
        return await orders.GetById(id);
    }

    public async Task<OrderDto?> GetOrderDetail(Guid id)
    {
        var order = await orders.GetByIdWithItems(id);
        if (order == null)
        {
            return null;
        }

        // The detail breaks the total down, so it adds the items up again
        // here. Free delivery from 50.
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

    public async Task<OrderDto> CreateOrder(OrderDto request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new Exception("Order must contain at least one item");
        }

        // The customer relation was bolted on later, so we match by e-mail and
        // create the customer if this is their first order.
        var customer = await customers.GetByEmail(request.CustomerEmail);
        if (customer == null)
        {
            customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = request.CustomerName,
                Email = request.CustomerEmail,
                Street = request.Street,
                City = request.City,
                Zip = request.Zip,
                RegisteredAt = DateTime.Now,
            };
            await customers.Add(customer);
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
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
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = requested.Quantity,
            });

            itemsTotal = itemsTotal + product.Price * requested.Quantity;
        }

        if (itemsTotal < 50)
        {
            itemsTotal = itemsTotal + 4.99m;
        }

        await orders.Add(order);

        await email.SendOrderConfirmation(order);

        var dto = order.ToDto();
        dto.Total = Math.Round(itemsTotal, 2);
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
        Guid id, string customerEmail, string returnUrl)
    {
        var order = await orders.GetById(id);
        if (order == null || order.CustomerEmail != customerEmail)
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
            if (previous.Status == "Created")
            {
                return (order.PaymentId.Value, previous.RedirectUrl);
            }
            if (previous.Status == "Captured")
            {
                throw new Exception("The order is already paid");
            }
        }
        var attempt = order.PaymentId == null ? order.Id.ToString() : $"{order.Id}/{order.PaymentId}";

        // The amount is whatever the detail shows, shipping included.
        var detail = await GetOrderDetail(id);
        var payment = await payments.CreatePayment(detail!.Total, "EUR", order.Id.ToString(), returnUrl, attempt);

        order.PaymentId = payment.PaymentId;
        await orders.Update(order);
        return payment;
    }

    public async Task<string> ConfirmPayment(Guid id, Guid paymentId)
    {
        var order = await orders.GetById(id);
        if (order == null || order.PaymentId != paymentId)
        {
            throw new Exception("Unknown payment");
        }

        // Never trust the redirect: ask the provider what really happened.
        var status = await payments.GetStatus(paymentId);
        if (status == "Captured" && order.Status == OrderStatus.Cancelled)
        {
            // Paid for an order that was cancelled meanwhile: the money goes back.
            return await payments.CancelOrRefund(paymentId);
        }
        if (status != "Captured" || order.Status != OrderStatus.New)
        {
            return status;
        }

        using var transaction = await db.Database.BeginTransactionAsync();

        order.Status = OrderStatus.Paid;
        await orders.Update(order);

        await transaction.CommitAsync();

        audit.Record("OrderPaid", order.Id);
        return status;
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

        // No Id: EF generates it, so Update() inserts the item instead of
        // updating a row that does not exist yet.
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

        order.Items.Remove(item);
        await stock.Release(item.ProductId, item.Quantity);
        await orders.Update(order);
    }

    public async Task<List<OrderDto>> GetOrdersForCustomer(string email)
    {
        var all = await orders.GetAll();
        var result = new List<OrderDto>();
        foreach (var order in all)
        {
            if (order.CustomerEmail == email)
            {
                result.Add(order.ToDto());
            }
        }
        return result;
    }

    public async Task<int> GetOrderCountByStatus(string status)
    {
        // The endpoint passes whatever the caller typed.
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
