using Kofi.Domain.Shared;

namespace Kofi.Domain.Subscriptions;

// One product in a prepared delivery: the planned coffee or an extra.
public record DeliveryLine(Guid ProductId, string ProductName, Quantity Quantity, Price UnitPrice);
