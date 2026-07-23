using MediatR;

namespace Kofi.BusinessLayer.Abstractions;

// Our own vocabulary on top of MediatR. The markers carry no members — their
// whole job is to say what a message is, so the pipeline can treat writes and
// reads differently (transactions wrap commands, not queries) and so a handler
// declares its intent in its signature.

public interface ICommand<out TResponse> : IRequest<TResponse>;

public interface ICommand : IRequest;

public interface IQuery<out TResponse> : IRequest<TResponse>;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
