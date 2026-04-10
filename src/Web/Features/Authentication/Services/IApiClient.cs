using Contracts.Results;

namespace OpenPsa.Web.Features.Authentication.Services;

public interface IApiClient {
    Task<Result<T>> GetAsync<T>(string uri, bool suppressNotFound = false);
    Task<Result<T>> PostAsync<T>(string uri, object? body = null);
    Task<Result<T>> PutAsync<T>(string uri, object body);
    Task<Result<T>> PatchAsync<T>(string uri, object body);
    Task<Result<T>> DeleteAsync<T>(string uri);
}
