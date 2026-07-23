namespace Kofi.BusinessLayer.Abstractions;

// A rule of the business was broken — that is a 400, not a 500.
// The exception handler turns this into a ProblemDetails response.
public class DomainException(string message) : Exception(message);
