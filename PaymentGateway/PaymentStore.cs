namespace PaymentGateway;

// In memory on purpose: this is a sandbox, not a ledger.
public class PaymentStore
{
    private readonly object gate = new();
    private readonly Dictionary<Guid, Payment> payments = new();
    private readonly Dictionary<string, Guid> byIdempotencyKey = new();

    // Creating and storing happen together, so two requests with the same
    // key can never both create a payment.
    public Payment GetOrCreate(string idempotencyKey, Func<Payment> create)
    {
        lock (gate)
        {
            if (byIdempotencyKey.TryGetValue(idempotencyKey, out var existing))
            {
                return payments[existing];
            }
            var payment = create();
            payments[payment.Id] = payment;
            byIdempotencyKey[idempotencyKey] = payment.Id;
            return payment;
        }
    }

    public Payment? Find(Guid id)
    {
        lock (gate)
        {
            return payments.GetValueOrDefault(id);
        }
    }

    // Every change of status goes through here, so a payment approved on the
    // payment page and cancelled by the shop at the same moment ends up in
    // exactly one of the two states.
    public Payment? Move(Guid id, PaymentStatus from, PaymentStatus to)
    {
        lock (gate)
        {
            var payment = payments.GetValueOrDefault(id);
            if (payment?.Status == from)
            {
                payment.Status = to;
            }
            return payment;
        }
    }

    public List<Payment> All()
    {
        lock (gate)
        {
            return payments.Values.OrderByDescending(p => p.CreatedAt).ToList();
        }
    }
}
