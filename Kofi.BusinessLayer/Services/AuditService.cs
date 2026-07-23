using Microsoft.Extensions.Logging;

namespace Kofi.BusinessLayer.Services;

// Who did what, to which record or with which input, and when.
public class AuditService(ILogger<AuditService> logger, ICurrentUser user) : IAuditService
{
    public void Record(string action, Guid entityId)
    {
        logger.LogInformation("AUDIT {Action} {EntityId} by {User} at {At}",
            action, entityId, user.UserId?.ToString() ?? "anonymous", DateTime.Now);
    }

    public void Record(string action, string payload)
    {
        logger.LogInformation("AUDIT {Action} {Payload} by {User} at {At}",
            action, payload, user.UserId?.ToString() ?? "anonymous", DateTime.Now);
    }
}
