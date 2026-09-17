using Kofi.Common.Domain;
using MediatR;

namespace Kofi.Host;

public class TransactionalDomainEventPublisher(ModuleUnitOfWork work) : INotificationPublisher
{
    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlers, INotification notification, CancellationToken ct)
    {
        async Task<bool> Run()
        {
            foreach (var handler in handlers)
            {
                await handler.HandlerCallback(notification, ct);
            }

            return true;
        }

        if (notification is IDomainEvent)
        {
            await work.Run(Run, ct);
        }
        else
        {
            await Run();
        }
    }
}
