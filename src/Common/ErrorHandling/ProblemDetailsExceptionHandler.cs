using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.ErrorHandling;

/// <summary>
/// Global IExceptionHandler that maps DomainException and unexpected exceptions
/// into RFC 7807 ProblemDetails responses. Wires correlation id (TraceIdentifier)
/// into the response so client-side errors can be cross-referenced with logs.
/// </summary>
public sealed class ProblemDetailsExceptionHandler : IExceptionHandler {
    private readonly IProblemDetailsService _problemDetails;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ProblemDetailsExceptionHandler> _logger;

    public ProblemDetailsExceptionHandler(
        IProblemDetailsService problemDetails,
        IHostEnvironment environment,
        ILogger<ProblemDetailsExceptionHandler> logger) {
        _problemDetails = problemDetails;
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken) {

        var problem = BuildProblem(exception);
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        if (exception is DomainException) {
            // Expected — log at warning, no stack trace noise.
            _logger.LogWarning(
                "Domain error {ErrorType} on {Method} {Path}: {Message}",
                problem.Type, httpContext.Request.Method, httpContext.Request.Path, exception.Message);
        } else {
            _logger.LogError(exception,
                "Unhandled exception on {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            if (_environment.IsDevelopment()) {
                problem.Extensions["exception"] = new {
                    type = exception.GetType().FullName,
                    message = exception.Message,
                    stack = exception.StackTrace
                };
            }
        }

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ProblemDetails BuildProblem(Exception exception) {
        switch (exception) {
            case ValidationException ve:
                return new ValidationProblemDetails(ve.Errors.ToDictionary(kv => kv.Key, kv => kv.Value)) {
                    Type = ve.ErrorType,
                    Title = ve.Title,
                    Status = ve.StatusCode,
                    Detail = ve.Message,
                };

            case DomainException de:
                return new ProblemDetails {
                    Type = de.ErrorType,
                    Title = de.Title,
                    Status = de.StatusCode,
                    Detail = de.Message,
                };

            case BadHttpRequestException bre:
                return new ProblemDetails {
                    Type = "https://openpsa.dev/errors/bad-request",
                    Title = "Bad request",
                    Status = bre.StatusCode,
                    Detail = bre.Message,
                };

            default:
                return new ProblemDetails {
                    Type = "https://openpsa.dev/errors/internal",
                    Title = "An unexpected error occurred",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "The server encountered an unexpected condition.",
                };
        }
    }
}
