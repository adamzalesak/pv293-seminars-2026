using Kofi.Common.Abstractions;
using MediatR;

namespace Kofi.Host;

public class TransactionBehavior<TRequest, TResponse>(ModuleUnitOfWork work)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var command = request is ICommand ||
                      request.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
        return command ? work.Run(() => next(ct), ct) : next(ct);
    }
}
