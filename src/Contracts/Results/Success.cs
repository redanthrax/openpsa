namespace Contracts.Results;

public sealed class Success<T> : Result<T> {
    internal Success(T? data) : base(true, data, null) { }
}
