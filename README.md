# Seminar 2 — reference solution

Last seminar we designed KoFI — a small e-shop selling specialty coffee
gear — and wrote down two decisions:

- **ADR-0001** — one deployable system
- **ADR-0002** — n-layer inside the API, one layer set per entity

This is the code those decisions produced. It **works**: KoFI is live and
has paying customers. Four months from a blank folder to a shop that makes
money is a good outcome, and the architecture we chose is a large part of
why it was possible.

Today we look at what it costs us.

## Getting started

Prerequisites: .NET 10 SDK, Docker, Node.js 22+ and pnpm.

```bash
dotnet run --project Kofi.AppHost
```

The shop itself runs at http://localhost:5173 (`Kofi.Web`, a front end over the
API; see its README).

Open the Aspire dashboard URL printed in the console. From there you can
reach the `api` service and its logs, the API reference at `/scalar/v1`,
and the `mail` test inbox (MailPit), where every e-mail the shop sends
lands. Sample requests are in
[Kofi.API/kofi.http](Kofi.API/kofi.http).

The database starts empty and is seeded on every run, so you can always get
back to a known state by restarting.

### Logging in

Everything except browsing the catalogue (and the return from the payment
page) needs a token. There are two accounts, no sign-up:

| Role | E-mail | Password |
|---|---|---|
| Customer | `petra@example.com` | `Customer123!` |
| Administrator | `admin@kofi.test` | `Admin123!` |

`POST /auth/login` with `{ "email": ..., "password": ... }` returns an
`accessToken`. Send it as `Authorization: Bearer <token>` — in Scalar, paste
it into the authentication field once; in `kofi.http`, paste it into the
variables at the top. The customer places and pays orders and sees only
their own; the administrator sees everyone's. Everything else (shipping,
customers, product changes) is the administrator's.

No request carries a customer id: who you are always comes from the token.

These are local test credentials, not secrets.

### Paying

Payments go through `payments`, a stand-in for a card payment provider that
Aspire runs next to the shop (it moves no money). `POST /orders/{id}/pay`
answers with the provider's payment page (`redirectUrl`); after you press
**Pay** there, the provider sends you back to the shop, which asks the
provider for the payment's status before it marks the order paid. Without a
browser, `kofi.http` shows the same steps.

## Task 1: Read the code

Spend ten to fifteen minutes in the solution. Start with `Kofi.API/Program.cs`,
then `Kofi.BusinessLayer/Services/OrderService.cs` and
`Kofi.API/Endpoints/OrderEndpoints.cs`.

Write down **every place that makes you uncomfortable** — and next to each
one, what it would cost you to change something there. Bring the list; we
will compare notes.

Some things to try while reading:

- create an order through the API and find the confirmation e-mail in the
  `mail` inbox
- call `GET /orders` with different combinations of query parameters
- call the same failing operation on several endpoints and compare the
  shape of the error you get back

## Task 2: The ticket

> **[KOFI-118] Audit log for order creation**
>
> Cancelling an order writes an audit record. So does marking one paid.
> Creating one does not — nobody remembered to add the call. We want that
> record, and we want to stop relying on remembering.
>
> Also, the whole operation has to be atomic. In production it has happened
> a few times that stock was decremented but no order was created — support
> has the tickets to prove it.

Before writing any code, answer two questions:

1. How many files does this touch?
2. If we wanted the same audit record on *every* write operation, not just
   order creation, how many places would we have to remember?

You can reproduce the second half of the ticket yourself: place an order
where the first item is in stock and the second one is not. Watch the stock
level of the first product before and after.

## What is in here

```
Kofi.API/                 endpoints, one class per entity
Kofi.BusinessLayer/       services and DTOs
  Services/                  OrderService, ProductService, CustomerService,
                             StockService, PricingService, EmailService,
                             AuditService, PaymentGatewayClient
                             (each behind an interface)
Kofi.DataAccessLayer/     entities, DbContext, repositories
```

Three projects, referenced in a line: API → BusinessLayer → DataAccessLayer.
Exactly what ADR-0002 said we would build.

Worth checking while you read: could an endpoint in `Kofi.API/Endpoints`
inject `AppDbContext` and skip the service? What stops it?

## What this refactor did not fix

Whatever bothered you most in Task 1 is probably still here.

The price of an order is a rule several features need, so it lives in
`PricingService`. `CreateOrderHandler`, `EmailService` and the payment ask
it; the order list and detail in `OrderService` still have their own
copies until they move. The slices fixed the transaction boundary and the
cross-cutting concerns. They did not fix a missing domain model, because
that is not what a slice is for.

That is the next decision, not this one.

## One thing worth trying

Write a test for `CreateOrderHandler`. Then write the same test against
`OrderService.CreateOrder`, which is still in the repo.

Count what you have to stand up in each case. That number is the argument
this seminar could not fit into a slide.
