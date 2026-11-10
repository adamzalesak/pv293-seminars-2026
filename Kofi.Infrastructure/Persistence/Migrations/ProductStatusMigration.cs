using Microsoft.EntityFrameworkCore;

namespace Kofi.Infrastructure.Persistence.Migrations;

public static class ProductStatusMigration
{
    public static void Apply(AppDbContext db) =>
        db.Database.ExecuteSqlRaw("""
            DO $migration$
            BEGIN
                IF EXISTS (SELECT 1 FROM information_schema.columns
                    WHERE table_schema='public' AND table_name='Products' AND column_name='IsActive') THEN
                    ALTER TABLE public."Products" ADD COLUMN "Status" text;
                    UPDATE public."Products" SET "Status"=CASE WHEN "IsActive" THEN 'Active' ELSE 'Inactive' END;
                    ALTER TABLE public."Products" ALTER COLUMN "Status" SET NOT NULL;
                    ALTER TABLE public."Products" DROP COLUMN "IsActive";
                END IF;
            END;
            $migration$;
            """);
}
