using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Common.Observability;

/// <summary>
/// Correlation ID middleware: reads X-Correlation-Id from the inbound request (or generates one),
/// stamps it on response headers, HttpContext.TraceIdentifier, and the Serilog LogContext so every
/// log line emitted during the request includes a CorrelationId property.
/// Register early in the pipeline, before UseSerilogRequestLogging.
/// </summary>
public sealed class CorrelationIdMiddleware {
    public const string HeaderName = "X-Correlation-Id";
    private const string LogPropertyName = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context) {
        var correlationId = ResolveCorrelationId(context);

        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() => {
            // Set only once; preserve any value already written.
            if (!context.Response.Headers.ContainsKey(HeaderName))
                context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(LogPropertyName, correlationId))
            await _next(context);
    }

    private static string ResolveCorrelationId(HttpContext context) {
        if (context.Request.Headers.TryGetValue(HeaderName, out var existing)) {
            var value = existing.ToString();
            if (!string.IsNullOrWhiteSpace(value) && value.Length <= 128)
                return value;
        }
        return Guid.NewGuid().ToString("N");
    }
}

public static class CorrelationIdMiddlewareExtensions {
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
