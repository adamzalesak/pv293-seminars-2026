using System.Reflection;
using FluentValidation;
using Kofi.Application.Behaviors;
using Kofi.Application.Features.Subscriptions.ProcessDueDeliveries;
using Kofi.Application.Persistence;
using Kofi.Application.Repositories;
using Kofi.Application.Services;
using Kofi.Common.Bootstrap;
using Kofi.Infrastructure.Persistence;
using Kofi.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Kofi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRemainingFeatures(this IServiceCollection services, ICollection<Assembly> assemblies)
    {
        assemblies.Add(typeof(IApplicationDbContext).Assembly);

        services.AddDbContext<AppDbContext>((sp, o) => o.UseNpgsql(sp.GetRequiredService<NpgsqlConnection>()));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ISubscriptionOrderService, SubscriptionOrderService>();
        services.AddScoped<DueDeliveryProcessor>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SubscriptionOwnershipBehavior<,>));
        services.AddValidatorsFromAssembly(typeof(IApplicationDbContext).Assembly);

        services.AddScoped<IModuleInitializer, RemainingInitializer>();

        return services;
    }
}
