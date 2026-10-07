using System.Reflection;
using FluentValidation;
using Kofi.Catalog.API.Endpoints;
using Kofi.Catalog.Application;
using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Application.Repositories;
using Kofi.Catalog.Contracts;
using Kofi.Catalog.Infrastructure.Persistence;
using Kofi.Catalog.Infrastructure.Persistence.Repositories;
using Kofi.Common.Bootstrap;
using Kofi.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kofi.Catalog;

public static class ModuleRegistration
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, ICollection<Assembly> moduleAssemblies)
    {
        moduleAssemblies.Add(typeof(ModuleRegistration).Assembly);

        services.AddDbContext<CatalogDbContext>((sp, options) => options.UseNpgsql(sp.GetRequiredService<NpgsqlConnection>()));
        services.AddScoped<ICatalogDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());
        services.AddScoped<AggregateDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());

        services.AddScoped<ICatalog, CatalogModule>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        services.AddScoped<IModuleInitializer, ModuleInitializer>();

        return services;
    }

    public static WebApplication MapCatalogModule(this WebApplication app)
    {
        app.MapProductEndpoints();
        return app;
    }
}
