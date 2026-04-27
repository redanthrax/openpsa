namespace Common.ErrorHandling;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Base type for application-level expected errors. The global exception handler
/// translates these into RFC 7807 ProblemDetails responses.
/// </summary>
public abstract class DomainException : Exception {
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }

    public abstract int StatusCode { get; }
    public abstract string ErrorType { get; }
    public virtual string Title => GetType().Name;
}

public sealed class NotFoundException : DomainException {
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.") {
        Resource = resource;
        Key = key;
    }

    public string Resource { get; }
    public object Key { get; }
    public override int StatusCode => StatusCodes.Status404NotFound;
    public override string ErrorType => "https://openpsa.dev/errors/not-found";
    public override string Title => "Resource not found";
}

public sealed class ConflictException : DomainException {
    public ConflictException(string message) : base(message) { }
    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string ErrorType => "https://openpsa.dev/errors/conflict";
    public override string Title => "Conflict";
}

public sealed class ForbiddenException : DomainException {
    public ForbiddenException(string message) : base(message) { }
    public override int StatusCode => StatusCodes.Status403Forbidden;
    public override string ErrorType => "https://openpsa.dev/errors/forbidden";
    public override string Title => "Forbidden";
}

public sealed class UnauthorizedException : DomainException {
    public UnauthorizedException(string message = "Authentication required") : base(message) { }
    public override int StatusCode => StatusCodes.Status401Unauthorized;
    public override string ErrorType => "https://openpsa.dev/errors/unauthorized";
    public override string Title => "Unauthorized";
}

public sealed class ValidationException : DomainException {
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.") {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
    public override int StatusCode => StatusCodes.Status400BadRequest;
    public override string ErrorType => "https://openpsa.dev/errors/validation";
    public override string Title => "Validation failed";
}
