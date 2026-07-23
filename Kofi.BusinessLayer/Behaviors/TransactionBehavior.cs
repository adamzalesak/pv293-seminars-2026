using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Data;

namespace Kofi.BusinessLayer.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(
    AppDbContext db,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var isCommand = request is ICommand || request.GetType()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        if (!isCommand)
        {
            return await next(cancellationToken);
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(cancellationToken);

            // Commit does not flush anything: EF only writes on SaveChanges.
            // Repositories that still save of their own accord land inside
            // this transaction; the ones we have cleaned up rely on this
            // line.
            await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogWarning("Rolled back {Request}", typeof(TRequest).Name);
            throw;
        }
    }
}
