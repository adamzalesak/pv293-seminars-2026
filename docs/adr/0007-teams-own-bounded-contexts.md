# ADR-0007: Teams own bounded contexts
Status: Accepted (Seminar 5)
## Context
KoFI needs more development teams. We need owners who understand their
business decisions and models. A split by technical layer creates
hand-offs for changes that span those layers.

Decision drivers, in priority order:
1. Team autonomy: an owner can make most internal decisions locally.
2. Clear ownership: models, language and business decisions have owners.
3. Coordination: collaboration across boundaries is explicit.
## Decision
For this iteration we choose Catalog, Warehouse, Ordering and
Subscriptions as bounded contexts. Each has one owning team; one team
may own more than one context. Customer and their delivery address
belong to Ordering. These are model and responsibility boundaries;
their technical implementation is a separate decision.
## Consequences
+ Disputed data and decisions have an owner.
+ Teams know which model and language they maintain.
- Work crossing a boundary still needs agreement between owners.
- The current shared model does not yet implement these boundaries.
