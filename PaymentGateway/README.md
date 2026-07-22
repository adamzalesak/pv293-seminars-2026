# PaymentGateway

A stand-in for an external card payment provider. It is **not part of KoFI**:
it moves no money and keeps payments in memory, so they disappear on restart.
It exists so that the shop talks to a real HTTP API the way it would talk to
a real provider. You do not need to change anything in here.

How the shop uses it:

- **Paying an order in the browser.** The shop creates a payment and sends the
  customer to `redirectUrl`, the provider's payment page. The page sends the
  customer back to the shop's return URL with `?paymentId=...`. The shop then
  asks the provider for the payment's status — it never trusts the URL alone.
- **Without a browser** (`kofi.http`): `POST /payments/{id}/approve` does what
  the Pay button on the page does.
- **Charging a saved card**: `POST /charges`, server to server, with no
  customer present.
- **Cancelling and refunding.** `POST /payments/{id}/cancel` stops a payment
  the customer has not made yet; `POST /payments/{id}/refund` returns a
  captured one. Neither changes a payment in any other state.

Test payment-method tokens for saved cards:

| Token | Result |
|---|---|
| `tok_visa` | approved |
| `tok_declined` | declined |
| `tok_timeout` | answers only after 30 s, so the caller times out first |

Requests with the same `idempotencyKey` return the same payment.

`GET /` lists every payment the provider has seen.
