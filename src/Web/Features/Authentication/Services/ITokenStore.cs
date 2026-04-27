using System.Text.Json;
using Microsoft.JSInterop;

namespace OpenPsa.Web.Features.Authentication.Services;

public interface ITokenStore {
    Task<string?> GetTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SetTokensAsync(string accessToken, string refreshToken);
    Task SetTokenAsync(string token);
    Task ClearAsync();

    /// <summary>
    /// Returns the current access token only when it is present AND its `exp` claim is
    /// still in the future (with a small skew). Returns null otherwise. Does NOT clear —
    /// the caller should attempt a refresh first.
    /// </summary>
    Task<string?> GetValidTokenAsync();

    /// <summary>True when the stored access token is missing or past its exp - skew.</summary>
    Task<bool> IsAccessTokenExpiredAsync();
}

public class LocalStorageTokenStore : ITokenStore {
    private const string AccessKey = "openpsa_token";
    private const string RefreshKey = "openpsa_refresh";
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromSeconds(30);
    private readonly IJSRuntime _js;

    public LocalStorageTokenStore(IJSRuntime js) => _js = js;

    public Task<string?> GetTokenAsync() =>
        _js.InvokeAsync<string?>("localStorage.getItem", AccessKey).AsTask();

    public Task<string?> GetRefreshTokenAsync() =>
        _js.InvokeAsync<string?>("localStorage.getItem", RefreshKey).AsTask();

    public async Task SetTokensAsync(string accessToken, string refreshToken) {
        await _js.InvokeVoidAsync("localStorage.setItem", AccessKey, accessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", RefreshKey, refreshToken);
    }

    public Task SetTokenAsync(string token) =>
        _js.InvokeVoidAsync("localStorage.setItem", AccessKey, token).AsTask();

    public async Task ClearAsync() {
        await _js.InvokeVoidAsync("localStorage.removeItem", AccessKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshKey);
    }

    public async Task<string?> GetValidTokenAsync() {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return null;
        return IsExpired(token) ? null : token;
    }

    public async Task<bool> IsAccessTokenExpiredAsync() {
        var token = await GetTokenAsync();
        return string.IsNullOrWhiteSpace(token) || IsExpired(token);
    }

    internal static bool IsExpired(string jwt) {
        try {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return true;
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            var rem = payload.Length % 4;
            if (rem == 2) payload += "==";
            else if (rem == 3) payload += "=";
            var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                Convert.FromBase64String(payload));
            if (json is null || !json.TryGetValue("exp", out var expEl)) return false;
            var exp = expEl.ValueKind == JsonValueKind.Number
                ? expEl.GetInt64()
                : long.Parse(expEl.GetString() ?? "0");
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp);
            return expiresAt - ExpirySkew <= DateTimeOffset.UtcNow;
        } catch {
            return true;
        }
    }
}
