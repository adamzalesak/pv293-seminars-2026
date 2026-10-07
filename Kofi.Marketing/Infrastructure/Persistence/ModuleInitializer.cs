using Kofi.Common.Bootstrap;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Infrastructure.Persistence;

internal sealed class ModuleInitializer(MarketingDbContext db) : IModuleInitializer
{
    public async Task InitializeSchema(CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);

        await using var check = db.Database.GetDbConnection().CreateCommand();
        check.CommandText = "SELECT to_regclass(@table) IS NOT NULL";
        var table = check.CreateParameter();
        table.ParameterName = "table";
        table.Value = "marketing.\"Coupons\"";
        check.Parameters.Add(table);

        if (!(bool)(await check.ExecuteScalarAsync(ct))!)
        {
            await db.Database.ExecuteSqlRawAsync(db.Database.GenerateCreateScript(), ct);
        }
    }

    public Task Seed(CancellationToken ct)
    {
        DemoData.Seed(db);
        return Task.CompletedTask;
    }
}
