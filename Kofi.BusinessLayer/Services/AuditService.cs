using Microsoft.Extensions.Logging;
namespace Kofi.BusinessLayer.Services;

public class AuditService(ILogger<AuditService> logger)
{
    // Whoever calls this remembers to call it. Some places do, some don't.
    public void Record(string action, Guid entityId)
    {
        logger.LogInformation("AUDIT {Action} {EntityId} at {At}",
            action, entityId, DateTime.Now);
    }
}
