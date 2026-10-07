using System.Reflection;
using FluentValidation;
using Kofi.Common.Bootstrap;
using Kofi.Common.Persistence;
using Kofi.Subscriptions.API.Endpoints;
using Kofi.Subscriptions.Application.Behaviors;
using Kofi.Subscriptions.Application.Features.Subscriptions.ProcessDueDeliveries;
using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Application.Repositories;
using Kofi.Subscriptions.Application.Services;
using Kofi.Subscriptions.Infrastructure.Persistence;
using Kofi.Subscriptions.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kofi.Subscriptions;

public static class ModuleRegistration
{
    public static IServiceCollection AddSubscriptionsModule(this IServiceCollection services, ICollection<Assembly> moduleAssemblies)
    {
        moduleAssemblies.Add(typeof(ModuleRegistration).Assembly);

        services.AddDbContext<SubscriptionsDbContext>((sp, options) => options.UseNpgsql(sp.GetRequiredService<NpgsqlConnection>()));
        services.AddScoped<ISubscriptionsDbContext>(sp => sp.GetRequiredService<SubscriptionsDbContext>());
        services.AddScoped<AggregateDbContext>(sp => sp.GetRequiredService<SubscriptionsDbContext>());

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionOrderService, SubscriptionOrderService>();
        services.AddScoped<DueDeliveryProcessor>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SubscriptionOwnershipBehavior<,>));
        services.AddValidatorsFromAssembly(typeof(ModuleRegistration).Assembly);

        services.AddScoped<IModuleInitializer, ModuleInitializer>();

        return services;
    }

    public static WebApplication MapSubscriptionsModule(this WebApplication app)
    {
        app.MapSubscriptionEndpoints();
        return app;
    }
}
