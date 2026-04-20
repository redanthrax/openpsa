using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Contracts.Results;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OpenPsa.Web.Features.Authentication.Services;

public partial class ApiClient : IApiClient {
    private static readonly TimeSpan[] RetryDelays = [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4)
    ];

    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;
    private readonly NavigationManager _nav;
    private readonly ISnackbar _snackbar;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient http, ITokenStore tokenStore, NavigationManager nav,
        ISnackbar snackbar, ILogger<ApiClient> logger) {
        _http = http;
        _tokenStore = tokenStore;
        _nav = nav;
        _snackbar = snackbar;
        _logger = logger;
    }

    public Task<Result<T>> GetAsync<T>(string uri, bool suppressNotFound = false) =>
        ExecuteAsync<T>("GET", uri, () => _http.GetAsync(uri), suppressNotFound);

    public Task<PagedResult<T>> GetPagedAsync<T>(string uri) =>
        ExecutePagedAsync<T>("GET", uri, () => _http.GetAsync(uri));

    public Task<Result<T>> PostAsync<T>(string uri, object? body = null) =>
        ExecuteAsync<T>("POST", uri, () => _http.PostAsJsonAsync(uri, body));

    public Task<Result<T>> PutAsync<T>(string uri, object body) =>
        ExecuteAsync<T>("PUT", uri, () => _http.PutAsJsonAsync(uri, body));

    public Task<Result<T>> PatchAsync<T>(string uri, object body) =>
        ExecuteAsync<T>("PATCH", uri, () => _http.PatchAsJsonAsync(uri, body));

    public Task<Result<T>> DeleteAsync<T>(string uri) =>
        ExecuteAsync<T>("DELETE", uri, () => _http.DeleteAsync(uri));

    public async Task<byte[]?> DownloadAsync(string uri) {
        try {
            await AttachTokenAsync();
            var response = await _http.GetAsync(uri);
            if (!response.IsSuccessStatusCode) {
                _snackbar.Add($"Download failed ({(int)response.StatusCode})", Severity.Error);
                return null;
            }
            return await response.Content.ReadAsByteArrayAsync();
        } catch (Exception ex) {
            LogException(_logger, "GET", uri, ex);
            _snackbar.Add("Download failed.", Severity.Error);
            return null;
        }
    }

    private async Task<Result<T>> ExecuteAsync<T>(string method, string uri,
        Func<Task<HttpResponseMessage>> action, bool suppressNotFound = false) {
        try {
            await AttachTokenAsync();
            var response = await RetryAsync(method, uri, action);

            if (response.StatusCode == HttpStatusCode.Unauthorized) {
                await _tokenStore.ClearAsync();
                _nav.NavigateTo("/login", forceLoad: false);
                return Result.Fail<T>("Session expired. Please log in again.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden) {
                const string msg = "You do not have permission to perform this action.";
                _snackbar.Add(msg, Severity.Error);
                return Result.Fail<T>(msg);
            }

            if ((int)response.StatusCode >= 500) {
                const string msg = "A server error occurred. Please try again.";
                LogError(_logger, method, uri, (int)response.StatusCode);
                _snackbar.Add(msg, Severity.Error);
                return Result.Fail<T>(msg);
            }

            if (!response.IsSuccessStatusCode) {
                string error;
                try {
                    var r = await response.Content.ReadFromJsonAsync<Result<T>>();
                    error = r?.Error ?? $"Request failed ({(int)response.StatusCode})";
                } catch {
                    error = $"Request failed ({(int)response.StatusCode})";
                }
                if (!(suppressNotFound && response.StatusCode == HttpStatusCode.NotFound))
                    _snackbar.Add(error, Severity.Error);
                return Result.Fail<T>(error);
            }

            var result = await response.Content.ReadFromJsonAsync<Result<T>>();
            if (result is { Success: false, Error: not null })
                _snackbar.Add(result.Error, Severity.Error);

            return result ?? Result.Fail<T>("Empty response.");
        } catch (Exception ex) {
            LogException(_logger, method, uri, ex);
            _snackbar.Add("A system error occurred.", Severity.Error);
            return Result.Fail<T>("A system error occurred.");
        }
    }

    private async Task<PagedResult<T>> ExecutePagedAsync<T>(string method, string uri,
        Func<Task<HttpResponseMessage>> action) {
        try {
            await AttachTokenAsync();
            var response = await RetryAsync(method, uri, action);

            if (response.StatusCode == HttpStatusCode.Unauthorized) {
                await _tokenStore.ClearAsync();
                _nav.NavigateTo("/login", forceLoad: false);
                return PagedResult.Fail<T>("Session expired. Please log in again.");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden) {
                const string msg = "You do not have permission to perform this action.";
                _snackbar.Add(msg, Severity.Error);
                return PagedResult.Fail<T>(msg);
            }

            if ((int)response.StatusCode >= 500) {
                const string msg = "A server error occurred. Please try again.";
                LogError(_logger, method, uri, (int)response.StatusCode);
                _snackbar.Add(msg, Severity.Error);
                return PagedResult.Fail<T>(msg);
            }

            if (!response.IsSuccessStatusCode) {
                string error;
                try {
                    var r = await response.Content.ReadFromJsonAsync<PagedResult<T>>();
                    error = r?.Error ?? $"Request failed ({(int)response.StatusCode})";
                } catch {
                    error = $"Request failed ({(int)response.StatusCode})";
                }
                _snackbar.Add(error, Severity.Error);
                return PagedResult.Fail<T>(error);
            }

            var result = await response.Content.ReadFromJsonAsync<PagedResult<T>>();
            if (result is { Success: false, Error: not null })
                _snackbar.Add(result.Error, Severity.Error);

            return result ?? PagedResult.Fail<T>("Empty response.");
        } catch (Exception ex) {
            LogException(_logger, method, uri, ex);
            _snackbar.Add("A system error occurred.", Severity.Error);
            return PagedResult.Fail<T>("A system error occurred.");
        }
    }

    private async Task AttachTokenAsync() {
        var token = await _tokenStore.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization = token is not null
            ? new AuthenticationHeaderValue("Bearer", token)
            : null;
    }

    private async Task<HttpResponseMessage> RetryAsync(string method, string uri,
        Func<Task<HttpResponseMessage>> action) {
        var response = await action();
        for (var i = 0; i < RetryDelays.Length; i++) {
            if (response.StatusCode is not (HttpStatusCode.BadGateway
                or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout))
                return response;
            LogRetry(_logger, method, uri, (int)response.StatusCode, i + 1);
            await Task.Delay(RetryDelays[i]);
            response = await action();
        }
        return response;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "{Method} {Uri} failed with HTTP {Status}")]
    private static partial void LogError(ILogger l, string method, string uri, int status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Method} {Uri} gateway error {Status}, retry {Attempt}")]
    private static partial void LogRetry(ILogger l, string method, string uri, int status, int attempt);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception on {Method} {Uri}")]
    private static partial void LogException(ILogger l, string method, string uri, Exception ex);
}
