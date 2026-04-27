using System.Net.Http.Json;

namespace OpenPsa.Web.Features.Authentication.Services;

public interface ITokenRefreshService {
    /// <summary>
    /// Attempts a single refresh-token exchange. Returns the new access token on success,
    /// or null when no refresh token is stored, the server rejected it, or the call failed.
    /// On success the token store is updated with the rotated pair.
    /// </summary>
    Task<string?> TryRefreshAsync();
}

/// <summary>
/// Calls /api/auth/refresh with its own HttpClient so it never recurses through ApiClient
/// (which would otherwise try to refresh during a refresh). Coalesces concurrent callers
/// onto a single in-flight request.
/// </summary>
public sealed class TokenRefreshService : ITokenRefreshService, IDisposable {
    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;
    private readonly ILogger<TokenRefreshService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Task<string?>? _inflight;

    public TokenRefreshService(HttpClient http, ITokenStore tokenStore,
        ILogger<TokenRefreshService> logger) {
        _http = http;
        _tokenStore = tokenStore;
        _logger = logger;
    }

    public Task<string?> TryRefreshAsync() {
        // Coalesce: if a refresh is already running, await the same task.
        if (_inflight is { IsCompleted: false }) return _inflight;
        return _inflight = RefreshCoreAsync();
    }

    private async Task<string?> RefreshCoreAsync() {
        await _gate.WaitAsync();
        try {
            var refresh = await _tokenStore.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refresh)) return null;

            using var resp = await _http.PostAsJsonAsync("/api/auth/refresh",
                new { RefreshToken = refresh });

            if (!resp.IsSuccessStatusCode) {
                // 401/400 => refresh token is invalid/expired/revoked; clear it so
                // subsequent requests fall through to a clean login redirect.
                await _tokenStore.ClearAsync();
                return null;
            }

            var body = await resp.Content.ReadFromJsonAsync<RefreshResponse>();
            if (body is null || string.IsNullOrWhiteSpace(body.Token)
                             || string.IsNullOrWhiteSpace(body.RefreshToken)) {
                await _tokenStore.ClearAsync();
                return null;
            }

            await _tokenStore.SetTokensAsync(body.Token, body.RefreshToken);
            return body.Token;
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Refresh-token exchange failed");
            return null;
        } finally {
            _gate.Release();
            _inflight = null;
        }
    }

    private sealed record RefreshResponse(string Token, string RefreshToken);

    public void Dispose() => _gate.Dispose();
}
