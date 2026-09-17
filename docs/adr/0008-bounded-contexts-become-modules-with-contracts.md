# ADR-0008: Bounded contexts become modules with contracts
Status: Accepted (Seminar 5)
Supersedes: ADR-0005
## Context
ADR-0007 assigned model and responsibility owners. Changing the delivery
address and replacing IsActive required fixes in other owners' code.
Consumers depend on internal model representations, so changes require
cross-team coordination even with clean dependency directions.

Decision drivers, in priority order:
1. Modifiability: internal representation changes should stay local.
2. Encapsulation: a context owns its model, decisions and data.
3. Operational simplicity: KoFI remains one deployable system.
## Decision
Implement the chosen contexts as modules. A module may reference
another module's Contracts assembly, never its implementation or
domain types. Contracts contain interfaces and their own DTOs.
Each module owns its DbContext and schema in one shared database.
The Kofi host composes modules through their registration entry points
and coordinates a local transaction when a
use case spans them. We start by completing Ordering over the existing
public tables; the remaining modules and schema moves follow in the
next sprint. Architecture tests will enforce the allowed
dependencies; they are introduced with the completed solution in S6.
## Consequences
+ Internal model changes can remain inside their owning module.
+ Public collaboration is explicit and the deployment remains simple.
- Contracts, mapping and more projects add maintenance work.
- Shared transactions keep runtime coordination between modules.
- A new question may still require a public contract change.
- Direct calls still make the caller know the parties it coordinates.
