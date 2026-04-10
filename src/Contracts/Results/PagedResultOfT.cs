namespace Contracts.Results;

public class PagedResult<T> {
    public bool Success { get; init; }
    public IEnumerable<T>? Data { get; init; }
    public string? Error { get; init; }
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    internal PagedResult(bool success, IEnumerable<T>? data, string? error, int totalCount, int pageNumber, int pageSize) {
        Success = success;
        Data = data;
        Error = error;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
