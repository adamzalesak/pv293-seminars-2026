using MediatR;

namespace Kofi.Domain;

// Something that happened in the business, recorded by the aggregate that
// decided it. It is a fact, so it is named in the past tense and carries no
// behaviour.
//
// The marker comes from MediatR: an in-process library with no I/O of its
// own. It is the single package this project allows, and swapping it out is
// a change in this one line.
public interface IDomainEvent : INotification;
