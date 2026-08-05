# Seminar 3 — the sprint landed

Last seminar we rewrote one use case live and wrote down two decisions:

- **ADR-0003** — features in the application layer, each use case owning its
  own request and response types
- **ADR-0004** — one database transaction per command

The team has since applied both to everything else. This is the result.

## What changed since you last looked

| | before | now |
|---|---|---|
| `OrderService` | 471 lines, 13 methods | gone |
| hand-written `BeginTransaction` | 8× | 0× — once, in a behavior |
| `SaveChangesAsync` in repositories | 6× | 0× — repositories only change what is tracked |
| `try/catch` in endpoints | 4 shapes of error | 0 — one shape, from the pipeline |
| files in `Kofi.BusinessLayer` | 32 | 96 |

A folder per use case under `Features/`, one type per file. That last row is
the price, and it is worth looking at before deciding whether the trade was
a good one.

## Getting started

Prerequisites: .NET 10 SDK, Docker, Node.js 22+ and pnpm.

```bash
dotnet run --project Kofi.AppHost
```

The shop itself runs at http://localhost:5173 (`Kofi.Web`, a front end over the
API; see its README).

Open the Aspire dashboard URL printed in the console: the API reference is at
`/scalar/v1`. E-mails land in the `mail` test inbox (MailPit, linked from the
dashboard); audit records show up in the logs.
Sample requests are in [Kofi.API/kofi.http](Kofi.API/kofi.http).

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

## Worth ten minutes before the seminar

Open `Features/Orders/CreateOrder/` and read the five files in it. Then open
`Behaviors/TransactionBehavior.cs`.

Two questions, the second one more interesting than the first:

1. Where does this use case decide to save? Where does it decide to commit?
2. The delivery rule — free over 50, otherwise 4.99 — is written out in four
   places. Tidying the structure did not tidy that. Why not?

## Today

No code. Business is coming with something none of us can describe yet, and
we are going to model it before anyone writes a line.
