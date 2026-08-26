namespace Kofi.Domain;

// The entry point of a consistency boundary. Everything inside it is loaded,
// changed and saved together, and other aggregates refer to it by Id.
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; protected set; }

    // Persistence moves it on whenever any part of the aggregate changes; the
    // domain itself never touches it.
    public int Version { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
