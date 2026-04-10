using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace OpenPsa.Web.Features.Authentication.Services;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider {
    private readonly ITokenStore _tokenStore;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthenticationStateProvider(ITokenStore tokenStore) =>
        _tokenStore = tokenStore;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
        var token = await _tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return Anonymous;

        try {
            var claims = ParseClaims(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        } catch {
            return Anonymous;
        }
    }

    public void NotifyStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static IEnumerable<Claim> ParseClaims(string jwt) {
        var payload = jwt.Split('.')[1]
            .Replace('-', '+').Replace('_', '/');
        var remainder = payload.Length % 4;
        if (remainder == 2) payload += "==";
        else if (remainder == 3) payload += "=";
        var bytes = Convert.FromBase64String(payload);
        var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bytes)!;

        var claims = new List<Claim>();
        foreach (var (key, value) in json) {
            if (value.ValueKind == JsonValueKind.Array) {
                foreach (var item in value.EnumerateArray())
                    claims.Add(new Claim(key, item.ToString()));
            } else {
                claims.Add(new Claim(key, value.ToString()));
            }
        }
        return claims;
    }
}
