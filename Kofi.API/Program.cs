using System.Globalization;
using FluentValidation;
using Kofi.API.Infrastructure;
using Kofi.BusinessLayer.Behaviors;
using Kofi.BusinessLayer.Features.Orders.CreateOrder;
using Kofi.DataAccessLayer.Data;
using Kofi.API.Auth;
using Kofi.API.Endpoints;
using Kofi.DataAccessLayer.Repositories;
using Kofi.BusinessLayer.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// No retrying execution strategy: it refuses transactions the application
// opens itself.
builder.AddNpgsqlDbContext<AppDbContext>("kofidb", settings => settings.DisableRetry = true);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(c => c.BaseAddress = new Uri("https+http://payments"));

// Audit is registered outside the transaction: inside it, a command that
// failed to commit would still leave an audit record saying it happened.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommand>();
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(AuditBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommand>();

// Messages follow the machine's locale by default; the codebase is English.
ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

builder.Services.AddOpenApi(options => options.AddBearerScheme());
builder.AddKofiAuth();

var app = builder.Build();

app.UseExceptionHandler();

// Product images live in wwwroot/images.
app.UseStaticFiles();
app.MapDefaultEndpoints();
app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapCustomerEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

app.Run();
