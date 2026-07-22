using System.Text.Json.Serialization;
using Kofi.DataAccessLayer.Data;
using Kofi.API.Auth;
using Kofi.API.Endpoints;
using Kofi.DataAccessLayer.Repositories;
using Kofi.BusinessLayer.Services;
using Scalar.AspNetCore;

// Dates are a mess (local vs UTC mixed all over the place). This switch makes
// Postgres accept whatever we send. Do NOT remove, everything breaks.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Retries are off: the retrying execution strategy refuses user-initiated
// transactions, and we start transactions by hand all over the place.
builder.AddNpgsqlDbContext<AppDbContext>("kofidb", settings => settings.DisableRetry = true);

builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CustomerRepository>();

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<PricingService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddHttpClient<PaymentGatewayClient>(c => c.BaseAddress = new Uri("https+http://payments"));

// Enums as text on the wire, same as in the database.
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.AddKofiAuth();

var app = builder.Build();

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

// TODO: move seeding somewhere proper
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

app.Run();
