namespace Kofi.API.Endpoints;

public record PaymentReturnResponse(Guid OrderId, string Status);
