# ADR-0003: Features in the application layer

**Status:** Accepted (Seminar 2)

**Supersedes:** ADR-0002

## Context

ADR-0002 organised the code in layers, one set per entity. Four months in,
that structure has a measurable price. `OrderService` has nine constructor
dependencies, thirteen methods and 472 lines, and it is the only place a
reader can look for anything about orders. It holds reads and writes, admin
screens and CSV exports, side by side. Every new use case lands in the class
named after the entity it touches, so those classes only grow.

None of this is a violation of ADR-0002. It is what ADR-0002 asked for: the
structure mirrors the data model rather than what the application does.

The same applies to what crosses the boundary. `OrderDto` is posted to
create an order and returned when one is read, so half its fields are
ignored on the way in and half stay empty on the way out, and nothing in
the type says which half. Adding a field for one screen adds it to every
caller.

The alternatives we considered: keep the services and split them by size
(arbitrary, and the next use case re-merges them); extract helpers from the
services (moves code around without changing where a reader looks first).

Decision drivers, in priority order:

1. Locality of change: one use case has one visible home.
2. Modifiability: changing one workflow should not disturb its neighbors.
3. Policy reuse: cross-cutting rules need one extension point.

## Decision

- Organise application operations by use case in `Features/<area>/`.
- Represent writes as commands and reads as queries.
- **Each use case defines its own request and response types.** No type is
  shared between two use cases, and none is shared between a read and a write.
- Shared application policies may use MediatR pipeline behaviors.
- Keep the current deployable and project boundaries.

## Consequences

+ A change has a visible home, and handlers keep fewer concerns.
+ The list of handlers is the list of things the system does.
+ A handler has one public entry point, so it can be exercised without
  standing up a whole service.
− More types: thirteen methods become thirteen handlers.
− Mediator indirection. `mediator.Send(command)` breaks "go to caller"; the
  handler is found by name, not by following a reference.
− Behavior ordering becomes significant, and nothing in the code makes that
  obvious.
− Many more small types, and two use cases returning the same fields will
  declare them twice. We accept that duplication: it is what lets one use
  case change without asking who else is looking.
− Two structures coexist until the migration finishes.
