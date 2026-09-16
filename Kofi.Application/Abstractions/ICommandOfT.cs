using MediatR;

namespace Kofi.Application.Abstractions;

public interface ICommand<out TResponse> : IRequest<TResponse>;
