using System.Reflection;
using Kofi;
using Kofi.Endpoints;
using Kofi.Infrastructure;
using Kofi.Ordering;

var builder = WebApplication.CreateBuilder(args);
builder.AddKofiHost();
var moduleAssemblies = new List<Assembly>();
builder.Services.AddOrderingModule(moduleAssemblies);
builder.Services.AddRemainingFeatures(moduleAssemblies);
builder.Services.AddKofiPipeline(moduleAssemblies);

var app = builder.Build();
app.UseKofiHost();
app.MapOrderingModule();
app.MapProductEndpoints();
app.MapSubscriptionEndpoints();
await app.InitializeModulesAsync();
app.Run();
