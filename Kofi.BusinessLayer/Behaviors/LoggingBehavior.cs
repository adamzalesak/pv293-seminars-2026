using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kofi.BusinessLayer.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling {Request}", name);

        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("Handled {Request} in {Elapsed} ms", name, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Request} failed after {Elapsed} ms", name, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
