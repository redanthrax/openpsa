namespace Contracts.Results;

public static class PagedResult {
    public static PagedResult<T> Ok<T>(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize) =>
        new(true, data, null, totalCount, pageNumber, pageSize);

    public static PagedResult<T> Fail<T>(string error) =>
        new(false, null, error, 0, 0, 0);
}
