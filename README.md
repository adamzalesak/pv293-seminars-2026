# KoFI

The shop has applied ADR-0006 across the application. The Kofi host composes
Application and Infrastructure; Application depends on Domain and uses
repository ports for commands and `IApplicationDbContext` for projections.
EF configurations and concrete repositories live in Infrastructure/Persistence.
Order, Customer, Product and Subscription protect their own state. Product
still owns its offer and available stock. Shipping addresses are three strings.

`Kofi.Domain` references MediatR but no infrastructure. Application uses
EF Core for query projections, an explicit dependency. Persistence moves an
aggregate's version when any owned part changes. Stale writes return 409.

Available stock is subtracted when reserved and returned on cancellation
or item removal. Shipping does not subtract it again.

The current process and aggregate data maps are in
`docs/event-storming/kofi-big-picture.excalidraw` and `.svg`.

```sh
dotnet test Kofi.Domain.Tests/Kofi.Domain.Tests.csproj
```

## Run locally

Install .NET 10 SDK, Docker and Node.js/pnpm, then run:

```sh
dotnet run --project Kofi.AppHost
```

Aspire starts PostgreSQL, the external payment-provider stand-in, MailPit,
the API and the web client. Local development uses HTTP. The provider is
not part of KoFI. Without SMTP configuration, e-mails are logged.

| Role | Email | Password |
|---|---|---|
| Customer | petra@example.com | Customer123! |
| Administrator | admin@kofi.test | Admin123! |

The customer identity comes from the token. Use `Kofi/kofi.http` for
HTTP requests. The saved card is a provider token, never card details.

PostgreSQL timestamps use local time. Seed data uses fixed product/customer
IDs and dates relative to startup; restarting preserves existing data.

## Host and module registration

The executable web host is Kofi; Kofi.AppHost orchestrates the local Aspire
application. The default web port is 5173; `Kofi__WebPort` overrides it for
verification without stopping another running frontend.

Host-level technical coordination lives in Kofi/Host under namespace Kofi.Host.
