using System.Reflection;
using Kofi;
using Kofi.Catalog;
using Kofi.Marketing;
using Kofi.Ordering;
using Kofi.Subscriptions;
using Kofi.Warehouse;

var builder = WebApplication.CreateBuilder(args);
builder.AddKofiHost();
var moduleAssemblies = new List<Assembly>();
builder.Services.AddWarehouseModule(moduleAssemblies);
builder.Services.AddOrderingModule(moduleAssemblies);
builder.Services.AddSubscriptionsModule(moduleAssemblies);
builder.Services.AddMarketingModule(moduleAssemblies);
builder.Services.AddCatalogModule(moduleAssemblies);
builder.Services.AddKofiPipeline(moduleAssemblies);

var app = builder.Build();
app.UseKofiHost();
app.MapWarehouseModule();
app.MapOrderingModule();
app.MapSubscriptionsModule();
app.MapMarketingModule();
app.MapCatalogModule();
await app.InitializeModulesAsync();
app.Run();
