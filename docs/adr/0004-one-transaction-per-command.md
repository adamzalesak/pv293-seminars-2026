# ADR-0004: One database transaction per command

**Status:** Accepted (Seminar 2)

## Context

The repositories call `SaveChangesAsync` as soon as they are asked to change
anything — six times across three classes. Each of those saves is its own
transaction, so an operation that writes more than once can fail halfway and
leave the earlier writes behind. `CreateOrder` is the case that reached
production: it reserves stock item by item, and when a later item is
unavailable it throws, having already committed the earlier reservation.
Stock disappears and no order exists (KOFI-118).

Eight other methods do open a transaction by hand. `CreateOrder`, the one
that needed it most, does not. A rule that every method has to remember is a
rule that some method will forget.

Decision drivers, in priority order:

1. Transactional consistency: one command succeeds or fails as a unit.
2. Reliability: the transaction and final save cannot be forgotten.
3. Maintainability: handlers describe workflow, not transaction lifecycle.

## Decision

- Every `ICommand` runs through `TransactionBehavior`.
- The behavior applies the middleware pattern: begin, `next()`,
  `SaveChangesAsync`, commit.
- An exception rolls the database transaction back and continues upward.

## Consequences

+ Every command gets the same transaction and final-save implementation.
+ Handler code is simpler and cannot omit the transaction or
  `SaveChangesAsync`.
+ Repositories can stop saving on their own, one at a time, without
  breaking the callers that have not moved yet.
− Individual commands have less freedom to choose a different transaction
  scope.
− Queries are excluded by a reflection check on every request.
− The retrying execution strategy refuses user-initiated transactions, so
  `DisableRetry = true` stays in `Program.cs`. Retrying a whole command
  would also repeat its e-mails and payment calls.
− The transaction covers only the database. An e-mail sent or a payment
  made inside a command is not undone when the command rolls back.
