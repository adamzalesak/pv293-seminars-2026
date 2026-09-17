namespace Kofi.Common.Bootstrap;

public interface IModuleInitializer
{
    Task InitializeSchema(CancellationToken ct);
    Task Seed(CancellationToken ct);
}
