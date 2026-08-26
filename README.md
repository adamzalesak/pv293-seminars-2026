# Seminar 4 — subscriptions are live

The team implemented ADR-0005 inside the existing application and database.
Each subscription operation is a feature under `Kofi.BusinessLayer/Features/Subscriptions/`,
using the same validation, audit and transaction pipeline as orders.

A customer subscribes to a coffee, a quantity, a delivery interval and the
day of the first delivery. The subscription starts Incomplete: the customer
first authorises their card for future charges on the payment provider's page
(`POST /subscriptions/{id}/card-setup`), the way a real provider such as
Stripe handles saved cards. The provider sends them back; the shop asks the
provider about the setup (`GET /subscriptions/{id}/card-return?setupId=...`),
keeps the card's token and activates the subscription, which plans the first
delivery; if it is due today, it reserves stock and creates and charges the
first order straight away. The next delivery is
recorded in the database; the customer can pause, resume, skip it, change the
plan or cancel. Subscriptions continue until cancelled, with no minimum term.
Each delivery holds its agreed base item and any one-off extras. Pause
keeps the waiting contents; on resume the customer picks the day of the next
delivery, today included. Skipping leaves the delivery in history and schedules
another one containing only the regular plan.
The card is the customer's, not the subscription's: every subscription is
charged to it. A customer who already saved a card activates a new
subscription straight away (`POST /subscriptions/{id}/activate`); authorising
a different card instead replaces the saved one for all their subscriptions. `GET /payment-card` shows it ("Visa ending 4242"); the customer
replaces it the same way (`POST /payment-card/setup`, back through
`GET /payment-card/return?setupId=...`). A charge the bank declines leaves the
delivery Scheduled for the next run, so a new card pays it.
Shipping remains free from 50 EUR, otherwise 4.99 EUR.

When the first delivery is due at activation, activation creates the first
order, its Ready delivery and one Scheduled delivery; a later first delivery
waits as Scheduled for its day. Ready means an order exists. A due delivery can
be prepared manually through
`POST /subscriptions/{id}/deliveries/{deliveryId}/prepare`; it creates the order
and schedules the next delivery from the previous planned date plus the interval.
Every day a cron job in our infrastructure calls
`POST /subscriptions/process-due`, which prepares every due delivery the same way,
one transaction per delivery; a delivery that cannot be prepared stays Scheduled for
the next run. Activation and preparation share `SubscriptionOrderService`, and
a subscription order is charged straight away with the customer's saved
card through the payment provider. Shipping uses the existing order
operation.
A delivery ends with its order: shipping and receipt are the order's, and the
subscription detail shows the order's status next to the delivery.

Extras can be added or removed only on a selected Scheduled delivery of an Active
or Paused subscription. They keep their accepted price, belong to that delivery,
and are not copied into the next one. Preparation checks the combined stock
of the base item and all extras; unavailable stock rejects the entire command.
Cancellation stops Scheduled deliveries; existing orders can still run their course.

## Getting started

Prerequisites: .NET 10 SDK, Docker, Node.js 22+ and pnpm.

```bash
dotnet run --project Kofi.AppHost
```

The shop itself runs at http://localhost:5173 (`Kofi.Web`, a front end over the
API; see its README).

Open the Aspire dashboard URL printed in the console. The API reference is at
`/scalar/v1`; sample requests are in [Kofi.API/kofi.http](Kofi.API/kofi.http).
E-mails land in the `mail` test inbox (MailPit, linked from the dashboard);
audit records appear in the logs.

Use a **fresh database** for this state. Startup creates and seeds its schema;
it does not migrate a database from an earlier seminar. Stop the earlier
AppHost and start this one with a new PostgreSQL resource.

### Logging in

Everything except browsing the catalogue needs a token. There are two
accounts, no sign-up:

| Role | E-mail | Password |
|---|---|---|
| Customer | `petra@example.com` | `Customer123!` |
| Administrator | `admin@kofi.test` | `Admin123!` |

`POST /auth/login` with `{ "email": ..., "password": ... }` returns an
`accessToken`. Send it as `Authorization: Bearer <token>` — in Scalar, paste
it into the authentication field once; in `kofi.http`, paste it into the
variables at the top. The customer places and pays orders and manages their
subscriptions (subscribe, authorise a card, pause, resume, skip, extras, plan, cancel). Order
and subscription lists show the customer only their own, the administrator
everyone's. Everything else (preparing deliveries,
shipping, customers, product changes) is the administrator's.
A customer can read and change only their own subscriptions; anyone else's
answers 404. No request carries a customer id: who you are always comes from
the token.

These are local test credentials, not secrets.

### Paying

Payments go through `payments`, a stand-in for a card payment provider that
Aspire runs next to the shop (it moves no money). `POST /orders/{id}/pay`
answers with the provider's payment page (`redirectUrl`); after you press
**Pay** there, the provider sends you back to the shop, which asks the
provider for the payment's status before it marks the order paid. Without a
browser, `kofi.http` shows the same steps.

Subscription orders need no browser: they are charged with the customer's
saved card (a token from the provider, never the card itself).

## Seeded customers

| Customer | Subscription |
|---|---|
| Petra Novakova (`petra@example.com`) | Active, with completed deliveries and one due today |
| Jan Dvorak (`jan@example.com`) | Paused, with delivery history |
| Eva Horakova (`eva@example.com`) | Cancelled, with delivery history |
| Miriam Vesela (`miriam@example.com`) | None yet |

Read `/customers`, `/products` and `/subscriptions` to obtain IDs. A delivery
with an `orderId` links to the order endpoints; a scheduled delivery has no order yet.

## Before the seminar

Compare two subscription handlers. Where does each one decide which state
allows the operation and when it is allowed? Then follow the entity references
from `Customer` through `Subscription` to its deliveries.

We kept the structure we agreed on. Today we examine what it protects.

During the seminar you will work in `Kofi.Domain`, which the application does
not use yet ([its README](Kofi.Domain/README.md)). Run
`dotnet test Kofi.Domain.Tests` beforehand so everything is downloaded; the
red tests are the exercise.
