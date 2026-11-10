using Kofi.Common.Bootstrap;
using Kofi.Infrastructure.Persistence.Migrations;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Infrastructure.Persistence;

internal sealed class RemainingInitializer(AppDbContext db) : IModuleInitializer
{
    public async Task InitializeSchema(CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);

        await using var check = db.Database.GetDbConnection().CreateCommand();
        check.CommandText = "SELECT to_regclass(@table) IS NOT NULL";
        var table = check.CreateParameter();
        table.ParameterName = "table";
        table.Value = "public.\"Products\"";
        check.Parameters.Add(table);

        if (!(bool)(await check.ExecuteScalarAsync(ct))!)
        {
            await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript(), ct);
        }

        ProductStatusMigration.Apply(db);
    }

    public Task Seed(CancellationToken ct)
    {
        DemoData.Seed(db);
        SubscriptionDemoData.Seed(db);

        return Task.CompletedTask;
    }
}
