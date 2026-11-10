using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kofi.Host;

public class ModuleUnitOfWork(IEnumerable<DbContext> contexts)
{
    private readonly IReadOnlyList<DbContext> _contexts = contexts.ToList();
    public bool IsActive => _contexts.Any(c => c.Database.CurrentTransaction != null);
    public IReadOnlyList<DbContext> Contexts => _contexts;

    public async Task<T> Run<T>(Func<Task<T>> action, CancellationToken ct)
    {
        if (IsActive)
        {
            return await action();
        }

        var first = _contexts[0];

        await using var transaction = await first.Database.BeginTransactionAsync(ct);

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

            await transaction.CommitAsync(ct);
            return response;
        }
        catch
        {
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
