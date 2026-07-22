namespace Kofi.BusinessLayer.Services;

public interface IAuditService
{
    void Record(string action, Guid entityId);
}
