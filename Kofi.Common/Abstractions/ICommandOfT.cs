using MediatR;

namespace Kofi.Common.Abstractions;

public interface ICommand<out TResponse> : IRequest<TResponse>;
