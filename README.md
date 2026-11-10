# KoFI

Ordering is the first extracted module (ADR-0008). It owns Order, Customer,
ShippingAddress, card references, payment and order handling. Its API,
Application, Domain and Infrastructure folders live in Kofi.Ordering;
Kofi.Ordering.Contracts exposes interfaces and plain DTOs. The remaining
application uses IOrdering.CreateOrder and customer/status queries rather
than accessing Ordering's model or tables.

Product (including stock) and Subscription remain in the shared Clean
projects. Product now has Draft / Active / Inactive / Discontinued states;
ShippingAddress accepts 60200 and 602 00 and stores 60200. Historical order
lines and addresses remain snapshots.

The Kofi host composes the module and the remaining application, including adapters
for Ordering's product and stock ports. AppDbContext owns products and
subscriptions; OrderingDbContext owns orders and customers. Both still use
public tables. One scoped connection and transaction covers their sequential
SaveChanges calls and a single commit. A failure rolls back both contexts;
external card charges and e-mails cannot be rolled back. No automatic retry.

Available stock is subtracted on reservation, returned on cancellation or
item removal, and is not subtracted again on shipment. Aggregate versions
advance for changes to owned children as well; stale writes return 409.

Ordering, Ordering.Contracts and Ordering.Tests are grouped in the Ordering
solution folder. Remaining domain tests belong to Kofi.Domain.Tests.

```sh
dotnet test Kofi.slnx
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

Each extracted module exposes Add<Module>Module and Map<Module>Module from
its root ModuleRegistration.cs. Registration contributes its assembly to
MediatR discovery and registers its validators, services, context and scoped
context alias. The host registers global pipeline behavior once. Modules
initialize and seed their own tables via IModuleInitializer; the host invokes
these participants without knowing the concrete contexts or seeders.

Product HTTP responses distinguish lifecycle status, sale eligibility and
new-subscription eligibility. The frontend supports sell-through, filters
subscription choices and validates the ShippingAddress input rules.

Host-level technical coordination lives in Kofi/Host under namespace Kofi.Host.
The Kofi.Infrastructure library (before full modularization) owns persistence
and adapters of the remaining application.
