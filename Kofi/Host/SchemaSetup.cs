using Npgsql;

namespace Kofi.Host;

public static class SchemaSetup
{
    public static async Task Apply(NpgsqlConnection connection, CancellationToken ct)
    {
        await connection.OpenAsync(ct);

        await using var check = connection.CreateCommand();
        check.CommandText = "SELECT to_regclass('public.\"Products\"') IS NOT NULL";

        if ((bool)(await check.ExecuteScalarAsync(ct))!)
        {
            await using var migrate = connection.CreateCommand();
            migrate.CommandText =
                await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Migrations", "001-module-schemas.sql"), ct);
            await migrate.ExecuteNonQueryAsync(ct);
        }
    }
}
