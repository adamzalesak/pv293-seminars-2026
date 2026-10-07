using System.Reflection;
using FluentValidation;
using Kofi.Common.Bootstrap;
using Kofi.Common.Persistence;
using Kofi.Marketing.API.Endpoints;
using Kofi.Marketing.Application;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Contracts;
using Kofi.Marketing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kofi.Marketing;

public static class ModuleRegistration
{
    public static IServiceCollection AddMarketingModule(this IServiceCollection services, ICollection<Assembly> moduleAssemblies)
    {
        moduleAssemblies.Add(typeof(ModuleRegistration).Assembly);

        services.AddDbContext<MarketingDbContext>((sp, options) => options.UseNpgsql(sp.GetRequiredService<NpgsqlConnection>()));
        services.AddScoped<IMarketingDbContext>(sp => sp.GetRequiredService<MarketingDbContext>());
        services.AddScoped<AggregateDbContext>(sp => sp.GetRequiredService<MarketingDbContext>());

        services.AddScoped<IMarketing, MarketingModule>();

        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        services.AddScoped<IModuleInitializer, ModuleInitializer>();

        return services;
    }

    public static WebApplication MapMarketingModule(this WebApplication app)
    {
        app.MapCouponEndpoints();
        return app;
    }
}
