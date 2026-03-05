# ADR-0002: Inside the API we go n-layer, with layers per entity

**Status:** Accepted (Seminar 1)

## Context

The team knows layered architecture from previous projects. We need to be on
the market quickly and the domain is not complicated yet. Starting from the
data model is the thing we can agree on fastest.

Decision drivers, in priority order:

1. Development speed: start without designing a new internal style.
2. Familiarity: every team member knows the layered pattern.
3. Consistency: one predictable place for each technical responsibility.

## Decision

Controller → Service → Repository, organised per entity, following the data
model.

## Consequences

- Everybody knows where to put things; a fast start with no new concepts to
  learn.
- The structure mirrors the data model rather than the behaviour of the
  domain.
- Service classes will grow with the number of use cases.
