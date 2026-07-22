namespace Kofi.API.Endpoints;

public record PaymentStartedResponse(Guid PaymentId, string RedirectUrl);
