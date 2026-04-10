namespace Contracts.Results;

public class Result<T> : Result {
    public T? Data { get; init; }

    public Result() { }

    protected Result(bool success, T? data, string? error) {
        Success = success;
        Data = data;
        Error = error;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2225:Operator overloads have named alternates",
        Justification = "Named alternatives Result.Ok<T> and Result.FromT<T> exist in base class")]
    public static implicit operator Result<T>(T data) => Result.Ok(data);
}
