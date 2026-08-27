# ADR-0006: Aggregates and an infrastructure-free domain project across KoFI

**Status:** Accepted (Seminar 4)

## Context

Subscription rules were duplicated across handlers and had already
drifted: a delivery already ordered and charged could still be marked
skipped. Pulling them into an aggregate fixed it, and the domain project
made the rules testable without a database.

Decision drivers, in priority order:

1. Domain correctness: invariants cannot be bypassed or drift apart.
2. Testability: business decisions run without infrastructure.
3. Enforceability: dependency direction is checked by the compiler.

## Alternatives

Keep the model local to subscriptions: less migration work, but different
approaches to rule ownership remain across the application.

## Decision

We apply the same treatment to the rest of the system. Every area gets
a domain model with its rules inside; the business layer becomes thin
orchestration; the domain project references none of our other projects
and no infrastructure.

## Consequences

+ Rules live next to the data they constrain, in one place.
+ The domain is testable without infrastructure, and the compiler
  enforces it.
− More ceremony, including in areas whose rules are trivial.
− Loading a subscription loads its whole delivery history, which grows
  over its lifetime.
− This says nothing about which parts of the system belong together.
