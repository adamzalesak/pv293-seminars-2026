using System.Reflection;
using FluentValidation;
using Kofi.Common.Bootstrap;
using Kofi.Common.Persistence;
using Kofi.Warehouse.API.Endpoints;
using Kofi.Warehouse.Application;
using Kofi.Warehouse.Application.Persistence;
using Kofi.Warehouse.Contracts;
using Kofi.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kofi.Warehouse;

public static class ModuleRegistration
{
    public static IServiceCollection AddWarehouseModule(this IServiceCollection services, ICollection<Assembly> moduleAssemblies)
    {
        moduleAssemblies.Add(typeof(ModuleRegistration).Assembly);

        services.AddDbContext<WarehouseDbContext>((sp, options) => options.UseNpgsql(sp.GetRequiredService<NpgsqlConnection>()));
        services.AddScoped<IWarehouseDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());
        services.AddScoped<AggregateDbContext>(sp => sp.GetRequiredService<WarehouseDbContext>());

        services.AddScoped<IWarehouse, WarehouseModule>();

        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        services.AddScoped<IModuleInitializer, ModuleInitializer>();

        return services;
    }

    public static WebApplication MapWarehouseModule(this WebApplication app)
    {
        app.MapStockEndpoints();
        return app;
    }
}
