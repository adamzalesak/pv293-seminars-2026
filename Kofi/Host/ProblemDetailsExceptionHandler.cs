using FluentValidation;
using Kofi.Application.Abstractions;
using Kofi.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Host;

public class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validation => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)),
            },
            // A rule of the domain said no: that is the caller's answer, not a
            // failure of ours.
            DomainRuleException rule => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Request cannot be processed",
                Detail = rule.Message,
            },
            NotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = notFound.Message,
            },
            // A body the model binder could not read is the caller's problem,
            // not ours. Without this arm it would leave here as a 500.
            BadHttpRequestException badRequest => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Malformed request",
                Detail = badRequest.Message,
            },
            // Someone else saved this aggregate after we loaded it. Retrying the
            // whole command could repeat its side effects, so the caller reloads
            // and decides again.
            DbUpdateConcurrencyException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Changed meanwhile",
                Detail = "The data changed after it was loaded. Reload it and try again.",
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Unexpected error",
            },
        };

        var status = problem.Status ?? StatusCodes.Status500InternalServerError;

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        context.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem,
            Exception = exception,
        });
    }
}
