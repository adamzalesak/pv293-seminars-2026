using System.Globalization;
using System.Reflection;
using FluentValidation;
using Kofi.Auth;
using Kofi.Common.Behaviors;
using Kofi.Common.Bootstrap;
using Kofi.Common.Services;
using Kofi.Host;
using Kofi.Ordering.Application.Ports;
using Kofi.Services;
using MediatR;
using Npgsql;
using Scalar.AspNetCore;

namespace Kofi;

public static class HostRegistration
{
    public static WebApplicationBuilder AddKofiHost(this WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        var services = builder.Services;

        services.AddScoped(_ => new NpgsqlConnection(builder.Configuration.GetConnectionString("kofidb")));
        services.AddScoped<ModuleUnitOfWork>();

        services.AddScoped<IEmailTransport, EmailTransport>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(c => c.BaseAddress = new Uri("https+http://payments"));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.AddScoped<IProductCatalog, LegacyProductCatalog>();
        services.AddScoped<IOrderStock, LegacyOrderStock>();

        services.AddProblemDetails();
        services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        services.AddOpenApi(options => options.AddBearerScheme());

        builder.AddKofiAuth();

        return builder;
    }

    public static IServiceCollection AddKofiPipeline(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies.ToArray());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuditBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            cfg.AddOpenBehavior(typeof(DomainEventBehavior<,>));
            cfg.NotificationPublisherType = typeof(TransactionalDomainEventPublisher);
        });

        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;

        return services;
    }

    public static WebApplication UseKofiHost(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStaticFiles();
        app.MapDefaultEndpoints();
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapAuthEndpoints();

        return app;
    }

    public static async Task InitializeModulesAsync(this WebApplication app, CancellationToken ct = default)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var initializers = services.GetServices<IModuleInitializer>().ToList();

        foreach (var initializer in initializers)
        {
            await initializer.InitializeSchema(ct);
        }

        await services.GetRequiredService<ModuleUnitOfWork>()
            .Run(async () =>
            {
                foreach (var initializer in initializers)
                {
                    await initializer.Seed(ct);
                }

                return true;
            }, ct);
    }
}
