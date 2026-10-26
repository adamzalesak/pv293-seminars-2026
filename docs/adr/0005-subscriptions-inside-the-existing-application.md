# ADR-0005: Subscriptions live inside the existing application

**Status:** Accepted (Seminar 3)

## Context

Subscriptions are a new area with their own vocabulary (plan,
delivery, pause) and a lifecycle that runs over months. Our event
storming wall shows that every delivery touches what we already
have: a delivery
becomes an order, it takes stock we sell from, and it charges the
customer. The wall also still has open hot spots; we do
not understand the process well yet. We are one team.

Options considered: the existing application and database; a separate
database inside the same application; a separate service.

Decision drivers, in priority order:

1. Tight contact points: the most frequent operation, preparing
   a delivery, touches ordering, stock and payments.
2. Reversibility: while the process is unclear, the boundary must be
   cheap to move.
3. Local consistency: the database changes of one command share one
   transaction.

## Decision

We implement subscriptions inside the existing application, next to
orders, using the feature structure and pipeline behaviors from
ADR-0003 and one transaction per command from ADR-0004.

## Consequences

+ Preparing a delivery creates the order and updates the subscription
  without a network call between them.
+ Moving the boundary later means moving folders, not splitting
  a database or a service.
+ The database changes of one command share one transaction.
− A subscription lives across many commands, and the card charge is
  an external call. Neither is covered by that transaction.
− Subscription code will reach into order and product entities
  directly, because nothing stops it.
− Nothing in the code marks where subscriptions end and ordering
  begins.
