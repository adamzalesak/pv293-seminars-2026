using MediatR;

namespace Kofi.Common.Abstractions;

public interface IQuery<out TResponse> : IRequest<TResponse>;
