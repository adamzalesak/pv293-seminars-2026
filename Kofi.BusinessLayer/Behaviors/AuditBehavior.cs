using System.Text.Json;
using MediatR;
using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;

namespace Kofi.BusinessLayer.Behaviors;

public class AuditBehavior<TRequest, TResponse>(IAuditService audit)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var isCommand = request is ICommand || request.GetType()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        var response = await next(cancellationToken);

        if (isCommand)
        {
            audit.Record(typeof(TRequest).Name, JsonSerializer.Serialize(request));
        }

        return response;
    }
}
