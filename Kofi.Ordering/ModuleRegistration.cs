using System.Reflection;
using FluentValidation;
using Kofi.Common.Bootstrap;
using Kofi.Ordering.API.Endpoints;
using Kofi.Ordering.Application;
using Kofi.Ordering.Application.Persistence;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Services;
using Kofi.Ordering.Contracts;
using Kofi.Ordering.Infrastructure.Persistence;
using Kofi.Ordering.Infrastructure.Persistence.Repositories;
using Kofi.Ordering.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kofi.Ordering;

public static class ModuleRegistration
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services, ICollection<Assembly> moduleAssemblies)
    {
        moduleAssemblies.Add(typeof(ModuleRegistration).Assembly);

        services.AddDbContext<OrderingDbContext>((sp, options) => options.UseNpgsql(sp.GetRequiredService<NpgsqlConnection>()));
        services.AddScoped<IOrderingDbContext>(sp => sp.GetRequiredService<OrderingDbContext>());
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<OrderingDbContext>());

        services.AddScoped<IOrdering, OrderingModule>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IOrderEmailService, OrderEmailService>();

        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        services.AddScoped<IModuleInitializer, ModuleInitializer>();

        return services;
    }

    public static WebApplication MapOrderingModule(this WebApplication app)
    {
        app.MapOrderEndpoints();
        app.MapCustomerEndpoints();
        app.MapPaymentCardEndpoints();

        return app;
    }
}
