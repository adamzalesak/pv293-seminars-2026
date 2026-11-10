namespace Kofi.Common.Services;

public interface IAuditService
{
    void Record(string action, Guid entityId);
    void Record(string action, string payload);
}
