namespace Kofi.Application.Abstractions;

// The thing asked for does not exist, or not for whoever is asking. The
// exception handler turns this into a 404.
public class NotFoundException(string message) : Exception(message);
