using MediatR;

namespace Kofi.Application.Abstractions;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;
