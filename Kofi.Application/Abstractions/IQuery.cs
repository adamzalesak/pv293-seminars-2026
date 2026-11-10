using MediatR;

namespace Kofi.Application.Abstractions;

public interface IQuery<out TResponse> : IRequest<TResponse>;
