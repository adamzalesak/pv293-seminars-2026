# Seminar 4 — the subscription owns its rules

The subscription rules now live behind one owner:
`Kofi.Domain/Subscriptions/Subscription.cs`. The aggregate decides which
change is legal from the state it is in, owns its deliveries, and keeps the
plan that was agreed. The handlers load it, call it and let the pipeline save.

`Kofi.Domain` references no project of ours and no infrastructure — no EF, no
ASP.NET. Its single package is MediatR, for the marker a published fact needs.
`Kofi.BusinessLayer` and `Kofi.DataAccessLayer` point at the domain, which is
the first concrete step of the dependency rule recorded in
[ADR-0006](docs/adr/0006-aggregates-and-an-infrastructure-free-domain-project.md).

What that bought us, compared with the state we started the seminar in:

- Skipping, changing the plan and editing extras touch only a Scheduled
  delivery. A delivery that already became an order stays as it was, and so
  do skipped ones.
- Deliveries and their extras can only be added or changed through the
  subscription.
- An order keeps the price it was placed at, whatever the price list does
  later; a delivery keeps the price it was planned with.
- Cancelling the subscription stops the Scheduled deliveries, not an order
  already placed: that order runs its course. Stopping it means cancelling
  the order.
- A command that decided over a stale copy is refused at write time: the
  subscription's version is checked in the `UPDATE`.

Activation no longer sends the welcome e-mail itself. The aggregate records
`SubscriptionActivated` and a separate handler reacts to it; local reactions
run inside the command's transaction, before `SaveChanges`.

The card is the customer's, not the subscription's: every subscription is
charged to it. A customer who already saved a card activates a new
subscription straight away (`POST /subscriptions/{id}/activate`); authorising
a different card instead replaces the saved one for all their subscriptions. `GET /payment-card` shows it ("Visa ending 4242"); the customer
replaces it the same way (`POST /payment-card/setup`, back through
`GET /payment-card/return?setupId=...`). A charge the bank declines leaves the
delivery Scheduled for the next run, so a new card pays it.
Subscriptions have no minimum term. Shipping is still free from 50 EUR,
otherwise 4.99 EUR.

The customer picks the day of the first delivery when subscribing. The
subscription starts Incomplete until the customer authorises a card for future
charges on the payment provider's page (`POST /subscriptions/{id}/card-setup`);
the provider sends them back and the shop asks it about the setup
(`GET /subscriptions/{id}/card-return?setupId=...`), keeps the card's token
and activates. Activation plans the first delivery for the chosen day (today
if it has passed); a delivery due today is prepared at once, so activation
then creates the first order, its Ready delivery and one Scheduled delivery.
Ready means the order exists. A due delivery is prepared manually through
`POST /subscriptions/{id}/deliveries/{deliveryId}/prepare`, using the same
`SubscriptionOrderService` as activation and the command's local transaction.
The next delivery starts from the processed delivery's planned date plus the interval.
A delivery ends with its order: shipping is the order's, and the subscription
detail shows the order's status next to the delivery. Every day a cron
job in our infrastructure calls `POST /subscriptions/process-due`, which
prepares every due delivery the same way, one transaction per delivery. Subscription
orders are charged straight away with the customer's saved card through the
payment provider.

Extras belong to one Scheduled delivery and can be edited only while the
subscription is Active or Paused. Pause preserves them; resume moves the waiting
delivery to the day the customer picks, today included. Skipping keeps its
contents in history and creates another delivery with only the regular plan.
Preparation checks combined stock for the base item and extras and uses their
accepted price snapshots.
Unavailable stock rejects the whole command.

Delivery history stays inside the aggregate. Loading it grows with the
subscription's lifetime; the teaching data assumes monthly or biweekly deliveries
for a few years. This is not a retention policy or a hard API limit.

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

The rules have tests that need nothing but the domain:

```bash
dotnet test Kofi.Domain.Tests
```

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
| Petra Novakova (`petra@example.com`) | Active, with delivery history and a delivery due today |
| Jan Dvorak (`jan@example.com`) | Paused, with delivery history and V60 filters in the waiting delivery |
| Eva Horakova (`eva@example.com`) | Cancelled, with delivery history |
| Miriam Vesela (`miriam@example.com`) | None yet |

Read `/customers`, `/products` and `/subscriptions` to obtain IDs. A delivery
with an `orderId` links to the order endpoints; a scheduled delivery has no
order yet.

## Before the next seminar

The rest of the system still works the way it did before. Pick any order or
product operation and ask the same question we asked here: where is the rule,
and who could get around it?
