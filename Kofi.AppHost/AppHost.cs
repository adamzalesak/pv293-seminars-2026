var builder = DistributedApplication.CreateBuilder(args);

// No data volume: every run starts from the seeded state.
var postgres = builder.AddPostgres("postgres");

var db = postgres.AddDatabase("kofidb");

// A test inbox: every e-mail the shop sends lands here, nothing leaves the machine.
var mail = builder.AddMailPit("mail");

// Stands in for the card payment provider; not part of KoFI.
var payments = builder.AddProject<Projects.PaymentGateway>("payments");

var api = builder.AddProject<Projects.Kofi_API>("api")
    .WithReference(db)
    .WithReference(mail)
    .WithReference(payments)
    .WaitFor(db);

// The shop's front end. Vite proxies /api to the API, so the browser sees one origin.
builder.AddViteApp("web", "../Kofi.Web")
    .WithPnpm()
    .WithEndpoint("http", e => e.Port = 5173)
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
