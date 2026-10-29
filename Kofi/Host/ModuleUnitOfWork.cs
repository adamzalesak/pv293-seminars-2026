using Kofi.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wolverine.EntityFrameworkCore;

namespace Kofi.Host;

public class ModuleUnitOfWork(IEnumerable<AggregateDbContext> contexts, IDbContextOutbox outbox)
{
    private readonly IReadOnlyList<AggregateDbContext> _contexts = contexts.ToList();
    private readonly List<object> _events = [];
    public bool IsActive => _contexts.Any(c => c.Database.CurrentTransaction != null);
    public IReadOnlyList<AggregateDbContext> Contexts => _contexts;

    // Events of the command, stored by Wolverine in the same transaction as the data.
    public void Publish(IEnumerable<object> events) => _events.AddRange(events);

    public async Task<T> Run<T>(Func<Task<T>> action, CancellationToken ct)
    {
        if (IsActive)
        {
            return await action();
        }

        var first = _contexts[0];

        await using var transaction = await first.Database.BeginTransactionAsync(ct);
        // The outbox writes its messages into this transaction. All module
        // contexts share it, so enrolling the first one is enough.
        outbox.Enroll(first);

        foreach (var context in _contexts.Skip(1))
        {
            await context.Database.UseTransactionAsync(transaction.GetDbTransaction(), ct);
        }

        try
        {
            var response = await action();

            // All contexts use the same connection; execute sequentially.
            foreach (var context in _contexts)
            {
                await context.SaveChangesAsync(ct);
            }

            // Stored together with the data: both are committed, or neither.
            foreach (var @event in _events)
            {
                await outbox.PublishAsync(@event);
            }

            _events.Clear();
            await transaction.CommitAsync(ct);

            // Only now do the stored messages go to their handlers.
            await outbox.FlushOutgoingMessagesAsync();

            return response;
        }
        catch
        {
            _events.Clear();
            await transaction.RollbackAsync(ct);
            throw;
        }
        finally
        {
            foreach (var context in _contexts.Skip(1))
            {
                await context.Database.UseTransactionAsync(null, ct);
            }
        }
    }
}
