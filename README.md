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

Modules announce what happened through integration events in their
Contracts. Commands and queries run on MediatR; every event, domain or
integration, is stored by Wolverine in the command's transaction and handed
to its handlers after the commit. Each handler runs on its own durable local
queue in its own transaction, so a crash or restart does not lose a message.
A failing handler is retried after 1, 2 and 4 seconds; then the message moves
to the dead letter queue (`wolverine.wolverine_dead_letters`).

Due deliveries are prepared by a Wolverine recurring message,
`PrepareDueDeliveries`, on the cron schedule in
`Subscriptions:DueDeliveriesCron` (daily at 6:00; every minute in
Development). `POST /subscriptions/process-due` runs the same by hand.

A due delivery becomes `Processing` and Subscriptions publishes
`DeliveryDueIntegrationEvent`. `SubscriptionOrderSaga` in Ordering, keyed by
the delivery id, reserves the stock in Warehouse (`ReserveStock`), places the
order and charges the saved card; the result comes by webhook. A declined
card is retried after `Ordering:PaymentRetryAfter` (a day), three tries in
all, and the customer is asked to update it. After the third decline the saga
cancels the order and releases the stock (`ReleaseStock`). Subscriptions
reacts to `OrderPaidIntegrationEvent` (delivery `Ready`) and to
`OrderFailedIntegrationEvent` or `OrderCancelledIntegrationEvent` (delivery
`Skipped`, e-mail); either way the next delivery is planned. Web checkout
does not use the saga.
Marketing issues the referral code on `SubscriptionActivatedIntegrationEvent`
and records a coupon use on `OrderPaidIntegrationEvent`. Catalog keeps its own
copy of running subscriptions per product and shows `ActiveSubscriptionCount`.

The payment provider calls `POST /payments/webhook` after every decision
about a payment, signed with HMAC-SHA256 (`X-Signature`) and retried until
it gets a 2xx answer. The shop stores it as `PaymentResultReceived` in a
durable queue before answering; its handler runs once per provider
`eventId` (`[Deduplicated]`). The webhook is the source of truth for the
payment; the customer's redirect back to the shop only shows the result
sooner. Both confirm the payment idempotently, by the order's state.

Handlers tolerate a message delivered twice: Marketing issues one referral
code per subscription and records a coupon use once per order, without
refusing it. Subscription events carry the subscription's state and version,
and Catalog's copy keeps the newest one, so events arriving out of order
cannot roll it back.

The `Demo` section in `Kofi/appsettings.json` slows down the referral coupon
handler (`MarketingDelayMs`) and makes the popularity handler in Catalog fail
(`PopularityFails`). Both are off by default.

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

Aspire 17 runs the app through the Aspire CLI. The first build fetches the
matching CLI with `dnx`, which comes with the .NET 10 SDK. If `dnx` is not on
your PATH (the .NET SDK from Homebrew), add the SDK directory to PATH, e.g.
`export PATH="$(brew --prefix dotnet)/libexec:$PATH"`, or install the CLI
from https://get.aspire.dev.

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

## Seminar 7 patches

The six seminar steps are in `patches/`. In the `seminar-7` start
checkout, apply them in order from the repository root. Run the commands
one at a time during the exercise so you can inspect each change. These
`git apply` commands work in Git Bash, PowerShell and Command Prompt.

```text
git apply patches/01-events-on-Wolverine-after-commit.patch
git apply patches/02-outbox.patch
git apply patches/03-duplicates.patch
git apply patches/04-retry.patch
git apply patches/05-recurring.patch
git apply patches/06-saga.patch
```

Each patch builds on the previous one and updates this README. To inspect
the current changes after any step, run `git diff --stat`. The commands
do not create commits. The `seminar-7-solution` state already contains
the result, so apply the patches only to `seminar-7`.
