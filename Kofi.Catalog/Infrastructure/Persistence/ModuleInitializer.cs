using Kofi.Common.Bootstrap;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Infrastructure.Persistence;

internal sealed class ModuleInitializer(CatalogDbContext db) : IModuleInitializer
{
    public async Task InitializeSchema(CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);

        await using var check = db.Database.GetDbConnection().CreateCommand();
        check.CommandText = "SELECT to_regclass(@table) IS NOT NULL";
        var table = check.CreateParameter();
        table.ParameterName = "table";
        table.Value = "catalog.\"Products\"";
        check.Parameters.Add(table);

        if (!(bool)(await check.ExecuteScalarAsync(ct))!)
        {
            await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript(), ct);
        }
    }

    public async Task Seed(CancellationToken ct)
    {
        DemoData.Seed(db);

        // The copy starts from the subscriptions that already run. This one-off
        // load reads the Subscriptions schema directly; from then on only the
        // published facts keep the copy current.
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO catalog."ProductSubscriptions" ("SubscriptionId", "ProductId", "IsActive", "Version")
            SELECT "Id", "ProductId", TRUE, "Version" FROM subscriptions."Subscriptions"
            WHERE "Status" IN ('Active', 'Paused')
            AND NOT EXISTS (SELECT 1 FROM catalog."ProductSubscriptions")
            """, ct);
    }
}
