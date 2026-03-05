# ADR-0001: KoFI will be one deployable system

**Status:** Accepted (Seminar 1)

## Context

New product, small team, first paying customer expected this quarter. Today
this means tens to hundreds of orders a day on the Czech market — on the
order of one order every few minutes. We intend to expand into the rest of
Europe, but we know neither when nor in what shape. We have no evidence that
any part of the system will need to scale independently of the rest. The
designs drawn in the seminar ranged from five services to a single
application.

Decision drivers, in priority order:

1. Time to market: reach the first paying customer this quarter.
2. Operational simplicity: a small team operates one deployable.
3. Transactional consistency: local data changes can share one transaction.

## Decision

One deployable system. Boundaries between areas are kept inside it, not
across the network.

## Consequences

- Simple deployment and operations; changes that cut across areas stay cheap.
- Consistency comes for free — one database, one transaction.
- Nothing enforces the boundaries, so they hold only as long as we are
  disciplined about them.
- Once there is real pressure to scale or release parts independently, this
  decision has to be revisited.
