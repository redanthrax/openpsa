using Common.ErrorHandling;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using DomainValidationException = Common.ErrorHandling.ValidationException;

namespace Common.Validation;

/// <summary>
/// Endpoint filter that runs every registered FluentValidation IValidator&lt;T&gt;
/// against each non-null argument of the endpoint delegate. On the first failure
/// throws DomainValidationException, which the global handler renders as a
/// ValidationProblemDetails (RFC 7807) response.
/// </summary>
public sealed class ValidationEndpointFilter : IEndpointFilter {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        var sp = context.HttpContext.RequestServices;

        foreach (var arg in context.Arguments) {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (sp.GetService(validatorType) is not IValidator validator) continue;

            var ctx = new ValidationContext<object>(arg);
            var result = await validator.ValidateAsync(ctx, context.HttpContext.RequestAborted);
            if (result.IsValid) continue;

            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new DomainValidationException(errors);
        }

        return await next(context);
    }
}

public static class ValidationFilterExtensions {
    /// <summary>
    /// Adds the FluentValidation endpoint filter so any registered IValidator&lt;T&gt;
    /// runs automatically against the matching delegate parameter.
    /// </summary>
    public static RouteHandlerBuilder WithValidation(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter<ValidationEndpointFilter>();

    /// <summary>
    /// Variant for grouped routes.
    /// </summary>
    public static RouteGroupBuilder WithValidation(this RouteGroupBuilder builder)
        => builder.AddEndpointFilter<ValidationEndpointFilter>();
}
