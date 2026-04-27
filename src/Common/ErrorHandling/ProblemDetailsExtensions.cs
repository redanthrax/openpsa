using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.ErrorHandling;

public static class ProblemDetailsExtensions {
    /// <summary>
    /// Registers the global ProblemDetails-based exception handler and the
    /// supporting ProblemDetails service. Call once in Program.cs.
    /// </summary>
    public static IServiceCollection AddProblemDetailsHandling(this IServiceCollection services) {
        services.AddProblemDetails(options => {
            options.CustomizeProblemDetails = ctx => {
                ctx.ProblemDetails.Instance ??= ctx.HttpContext.Request.Path;
                ctx.ProblemDetails.Extensions.TryAdd("traceId", ctx.HttpContext.TraceIdentifier);
            };
        });
        services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
        return services;
    }

    /// <summary>
    /// Wires the global exception handler into the pipeline. Must be registered
    /// BEFORE auth/authorization/endpoints, immediately after correlation id.
    /// </summary>
    public static IApplicationBuilder UseProblemDetailsHandling(this IApplicationBuilder app) {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }
}
