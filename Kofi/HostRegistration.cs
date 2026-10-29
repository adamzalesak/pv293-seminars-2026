using System.Globalization;
using System.Reflection;
using FluentValidation;
using JasperFx.Core;
using Kofi.Auth;
using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Infrastructure.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Common.Behaviors;
using Kofi.Common.Bootstrap;
using Kofi.Common.Demo;
using Kofi.Common.Domain;
using Kofi.Common.Services;
using Kofi.Host;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Infrastructure.Persistence;
using Kofi.Ordering.Application.Persistence;
using Kofi.Ordering.Contracts;
using Kofi.Ordering.Infrastructure.Persistence;
using Kofi.Services;
using Kofi.Subscriptions.Application.Features.Subscriptions.ProcessDueDeliveries;
using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Infrastructure.Persistence;
using Kofi.Warehouse.Application.Persistence;
using Kofi.Warehouse.Infrastructure.Persistence;
using MediatR;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;

namespace Kofi;

public static class HostRegistration
{
    public static WebApplicationBuilder AddKofiHost(this WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        var services = builder.Services;

        services.AddScoped(_ => new NpgsqlConnection(builder.Configuration.GetConnectionString("kofidb")));
        services.AddScoped<ModuleUnitOfWork>();
        services.Configure<DemoOptions>(builder.Configuration.GetSection("Demo"));

        services.AddScoped<IEmailTransport, EmailTransport>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(c => c.BaseAddress = new Uri("https+http://payments"));
        services.AddSingleton<IPaymentWebhookVerifier, PaymentWebhookVerifier>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

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
        });

        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;

        return services;
    }

    public static WebApplicationBuilder AddKofiMessaging(this WebApplicationBuilder builder, IEnumerable<Assembly> assemblies)
    {
        var connectionString = builder.Configuration.GetConnectionString("kofidb")!;
        // Wolverine's spans and metrics in the Aspire dashboard: a message
        // handled after the commit stays in the trace of the request that sent it.
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource("Wolverine"))
            .WithMetrics(metrics => metrics.AddMeter("Wolverine*"));

        builder.Host.UseWolverine(opts =>
        {
            // Every handler of an event runs on its own: its own queue and
            // transaction, so one failing reaction does not stop the others.
            opts.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

            // Wolverine finds its handlers by its own conventions (a name ending
            // in Handler, a saga) and our event handlers. MediatR's command and
            // query handlers look the same by name; they stay on MediatR.
            opts.Discovery.CustomizeHandlerDiscovery(q =>
            {
                q.Includes.WithCondition("Kofi event handler", IsEventHandler);
                q.Excludes.WithCondition("MediatR request handler", IsMediatRHandler);
            });
            foreach (var assembly in assemblies)
            {
                opts.Discovery.IncludeAssembly(assembly);
            }

            opts.PersistMessagesWithPostgresql(connectionString, "wolverine");
            opts.UseEntityFrameworkCoreTransactions()
                .WithDbContextAbstraction<ICatalogDbContext, CatalogDbContext>()
                .WithDbContextAbstraction<IWarehouseDbContext, WarehouseDbContext>()
                .WithDbContextAbstraction<IOrderingDbContext, OrderingDbContext>()
                .WithDbContextAbstraction<ISubscriptionsDbContext, SubscriptionsDbContext>()
                .WithDbContextAbstraction<IMarketingDbContext, MarketingDbContext>();
            opts.Policies.AutoApplyTransactions();
            // Domain events raised inside a Wolverine handler leave with its transaction.
            opts.PublishDomainEventsFromEntityFrameworkCore<AggregateRoot, IDomainEvent>(a => a.DomainEvents);
            // Typed HTTP clients are built by a factory the generated code cannot
            // inline; these are resolved from the container instead.
            opts.CodeGeneration.AlwaysUseServiceLocationFor<IOrdering>();
            opts.CodeGeneration.AlwaysUseServiceLocationFor<IPaymentGatewayClient>();

            // A message waits in the database until its handler is done, so a
            // crash does not lose it.
            opts.Policies.UseDurableLocalQueues();
            // One node: after a restart it takes over its stored messages at once.
            opts.Durability.Mode = DurabilityMode.Solo;
            // Storage for [Deduplicated]: ids a handler has already seen.
            opts.Durability.MessageDeduplicationMode = MessageDeduplicationMode.CompareByString;

            // Every day, instead of a cron job in our infrastructure. The
            // processor opens its own scopes, so it comes from the container.
            opts.CodeGeneration.AlwaysUseServiceLocationFor<DueDeliveryProcessor>();
            opts.Schedules.ScheduleRecurring<PrepareDueDeliveries>(
                builder.Configuration["Subscriptions:DueDeliveriesCron"] ?? "0 6 * * *");

            // A failing handler is tried again later, each time waiting twice
            // as long; a passing outage heals on its own. After the last try
            // the message waits in the dead letter queue for a person.
            // Production would wait minutes, not seconds.
            opts.OnAnyException()
                .ScheduleRetry(1.Seconds(), 2.Seconds(), 4.Seconds())
                .Then.MoveToErrorQueue();
        });

        return builder;
    }

    private static bool IsEventHandler(Type type) => Implements(type, typeof(IDomainEventHandler<>), typeof(IIntegrationEventHandler<>));
    private static bool IsMediatRHandler(Type type) => Implements(type, typeof(IRequestHandler<>), typeof(IRequestHandler<,>));

    private static bool Implements(Type type, params Type[] generics) =>
        type.GetInterfaces().Any(i => i.IsGenericType && generics.Contains(i.GetGenericTypeDefinition()));

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
        await SchemaSetup.Apply(services.GetRequiredService<NpgsqlConnection>(), ct);
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
