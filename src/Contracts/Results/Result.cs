namespace Contracts.Results;

public abstract class Result {
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static Result<T> Ok<T>(T data) => new Success<T>(data);
    public static Result<T> Fail<T>(string error) => new Failure<T>(error);
    public static Result<T> FromT<T>(T data) => Ok(data);
}
