# ADR-0009: Modules publish facts for independent reactions
Status: Accepted (Seminar 6)
## Context
Modules call each other only through contracts, yet subscription
activation calls Marketing and Catalog directly. Every new interested
module changes the activation handler although the activation decision
does not change.

Decision drivers, in priority order:
1. Modifiability: add a reaction without changing the publisher.
2. Modularity: subscribers own their reactions to public facts.
3. Transactional consistency: keep local database reactions atomic for now.
## Decision
For independent reactions, a module maps its domain event to an
integration event in its Contracts and publishes it; subscribers
register themselves. Delivery stays synchronous and in-process through
MediatR, inside the command transaction. Modules are independent by
default: between contexts we expect eventual consistency already now,
so the modules can later be split, and a module prefers its own copy
of data built from events. A synchronous call, command or query, needs
a strong reason, such as a coordinated process (IOrdering.CreateOrder)
or coupon validation, whose rules belong to Marketing.
## Consequences
+ A new interested module is added without touching the publisher.
+ The publisher depends on no subscriber; subscribers depend on its Contracts.
+ Local database reactions commit or roll back together.
- The flow is no longer visible in one place.
- A slow or failing subscriber delays or rolls back the publisher.
- External I/O, such as e-mail, is not rolled back.
- Local copies need initial loading and more events to keep them current.
