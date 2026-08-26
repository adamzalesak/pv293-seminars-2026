# Seminar 4: protect the subscription

Implement four methods in `Subscriptions/Subscription.cs`. The application
does not use this project yet; it is a model of subscriptions on its own,
with its tests in `Kofi.Domain.Tests`.
Everything else is prepared: the other transitions, deliveries, extras,
`Price`, `Quantity`, and the private helpers each TODO names. This exercise needs no
database; HTTP and EF mapping are shown on the running KoFI application.

Work alone or in pairs, in this order (easiest first). Nothing is graded;
what you do not finish we go through together.

1. `Pause` — only an Active subscription. The waiting delivery and its
   extras stay.
2. `AddExtra` — only the subscription's own `Scheduled` delivery, while the
   subscription is Active or Paused. The delivery checks quantity and
   currency itself.
3. `SkipNextDelivery` — only an Active subscription, its next `Scheduled`
   delivery, even when it is already due. It stays in history as `Skipped`;
   the next one is planned from its date plus the interval, from the plan
   alone, without its extras.
4. `ChangePlan` — an Active or Paused subscription. Only `Scheduled`
   deliveries follow the new plan; they keep their dates and extras, and
   the new interval counts from the next delivery on.

A rejected change leaves the state as it was.

## Run

From the repository root:

```sh
dotnet test Kofi.Domain.Tests
```

The tests are red until the four methods are done.
