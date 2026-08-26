namespace PaymentGateway;

// In memory on purpose, like PaymentStore.
public class SetupStore
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, CardSetup> setups = new();
    private readonly Dictionary<string, Guid> byIdempotencyKey = new();

    public CardSetup GetOrCreate(string idempotencyKey, Func<CardSetup> create)
    {
        lock (gate)
        {
            if (byIdempotencyKey.TryGetValue(idempotencyKey, out var existing))
            {
                return setups[existing];
            }
            var setup = create();
            setups[setup.Id] = setup;
            byIdempotencyKey[idempotencyKey] = setup.Id;
            return setup;
        }
    }

    public CardSetup? Find(Guid id)
    {
        lock (gate)
        {
            return setups.GetValueOrDefault(id);
        }
    }

    // What the customer may see about each test card.
    private static readonly Dictionary<string, string> labels = new()
    {
        ["tok_visa"] = "Visa ending 4242",
        ["tok_declined"] = "Visa ending 0002",
        ["tok_timeout"] = "Visa ending 0003",
    };

    // A setup is decided once; deciding it again changes nothing.
    public CardSetup? Decide(Guid id, SetupStatus status, string? token)
    {
        lock (gate)
        {
            var setup = setups.GetValueOrDefault(id);
            if (setup?.Status == SetupStatus.Created)
            {
                setup.Status = status;
                setup.PaymentMethodToken = status == SetupStatus.Succeeded ? token : null;
                setup.CardLabel = setup.PaymentMethodToken is { } saved ? labels.GetValueOrDefault(saved, "Test card") : null;
            }
            return setup;
        }
    }
}
