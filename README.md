# KoFI

KoFI remains one deployable system. Catalog, Warehouse, Ordering,
Subscriptions and Marketing own their models and database schemas. A module
references another module's Contracts assembly, never its implementation.
Contracts contain interfaces and plain DTOs. Common contains technical
abstractions.

Ordering owns Customer, the saved card reference, the delivery address and
orders. Subscription orders use `IOrdering.CreateOrder`: agreed lines in,
OrderId out. Order lines and addresses remain historical snapshots. Catalog
and Warehouse have different local Product models correlated by ProductId.
Only Warehouse has StockQuantity and ShelfLocation. Stock means available
quantity; reservation subtracts, release adds, shipping does not subtract again.

The Kofi host composes module implementations. Its ModuleUnitOfWork shares one scoped
connection and transaction, saves contexts sequentially and commits once.
A database failure rolls back all modules. External card charges and e-mails
are outside that rollback; commands are not automatically retried.

Marketing owns coupons. The administrator issues, edits and invalidates
them; activating a subscription gives the customer a referral code to pass on
(15 %, 90 days). A customer types a code when placing an order: Ordering asks
`IMarketing.ValidateCoupon` and keeps the code and the discount on the order.
The discount applies to the items, not to shipping. Each customer uses a
coupon once, and the owner of a referral code cannot use it.

New catalog products start Draft. `/activate` makes them Active,
`/discontinue` allows sell-through and existing deliveries but no new
subscriptions, and `/deactivate` makes them Inactive. ProductInfo exposes
consumer questions rather than the private status enum. ShippingAddress
accepts five digits with or without one space and stores e.g. 60200.

Startup creates module tables on a fresh database. For an existing S5
database, `Kofi/Migrations/001-module-schemas.sql` transactionally moves
the tables and separates stock data; it does not reset or reseed them. The Kofi host
owns this one-time cross-schema migration; contexts own only their local
mapping and schema. The table move is separate from changing ToTable.

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

## Module structure

Each Clean module has API (HTTP adapters), Application (use cases and ports),
Domain (model and rules) and Infrastructure (EF persistence and I/O) folders.
The separate Contracts assembly contains only public interfaces and DTOs.
Kofi remains the composition root for modules and shared transactions.

## Tests and solution folders

Each module has its own Kofi.<Module>.Tests project next to its implementation
and Kofi.<Module>.Contracts assembly in a solution folder. Unit and architecture
tests belong to that module; there is no shared Kofi.Tests project.

ArchUnitNET checks that modules do not depend on foreign implementations,
and Contracts do not expose module implementation or the technical kernel.
Rules select exact assemblies. Test projects reference the Kofi composition
root only to load all production assemblies for these checks.

Each module also checks its internal Clean Architecture dependencies:
Domain cannot depend on Application, Infrastructure, API, EF Core or ASP.NET;
Application cannot depend on Infrastructure or API; Infrastructure cannot
depend on API. EF Core query projections in Application are intentionally
allowed. Framework checks load the module's dependencies as well.

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
