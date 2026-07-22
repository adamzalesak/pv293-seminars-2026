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

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(c => c.BaseAddress = new Uri("https+http://payments"));

builder.Services.AddOpenApi(options => options.AddBearerScheme());
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

app.Run();
