namespace Contracts.Results;

public sealed class Failure<T> : Result<T> {
    internal Failure(string error) : base(false, default, error) { }
}
