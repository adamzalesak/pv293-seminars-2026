using MediatR;
using Kofi.DataAccessLayer.Data;
using Kofi.Domain;

namespace Kofi.BusinessLayer.Behaviors;

// Reactions to a domain event always write inside a transaction, wherever
// the event is published from. Inside a command they join its transaction
// and unit of work, so a failed reaction rolls the command back. Published
// on its own, from a background job for example, the event gets a
// transaction of its own.
public class TransactionalDomainEventPublisher(AppDbContext db) : INotificationPublisher
{
    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        if (notification is not IDomainEvent || db.Database.CurrentTransaction != null)
        {
            await Run(handlerExecutors, notification, cancellationToken);
            return;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await Run(handlerExecutors, notification, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task Run(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        foreach (var handler in handlerExecutors)
        {
            await handler.HandlerCallback(notification, cancellationToken);
        }
    }
}
