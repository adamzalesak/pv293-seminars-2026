namespace Kofi.Domain;

// A rule of the business said no. It is not a bug and not a 500 — the caller
// asked for something the current state does not allow.
public class DomainRuleException(string message) : Exception(message);
