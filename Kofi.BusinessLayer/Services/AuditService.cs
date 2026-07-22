using Microsoft.Extensions.Logging;
namespace Kofi.BusinessLayer.Services;

// Who did what to which record, and when.
public class AuditService(ILogger<AuditService> logger, ICurrentUser user)
{
    public void Record(string action, Guid entityId)
    {
        logger.LogInformation("AUDIT {Action} {EntityId} by {User} at {At}",
            action, entityId, user.Email ?? "anonymous", DateTime.Now);
    }
}
