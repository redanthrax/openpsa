using System.Net;
using System.Net.Http.Json;
using Contracts.Results;
using FluentAssertions;
using OpenPsa.Modules.Authentication.Features.Auth.Login;

namespace Api.Tests;

[Collection(IntegrationCollection.Name)]
public class RefreshTokenTests {
    private readonly OpenPsaFactory _factory;

    public RefreshTokenTests(OpenPsaFactory factory) {
        _factory = factory;
    }

    private async Task<LoginResponse> LoginAsync(HttpClient client) {
        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@openpsa.dev", "admin"));
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<Result<LoginResponse>>();
        return result!.Data!;
    }

    [Fact]
    public async Task Login_ReturnsAccessAndRefreshTokens() {
        var login = await LoginAsync(_factory.CreateClient());
        login.Token.Should().NotBeNullOrEmpty();
        login.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_WithValidToken_RotatesAndReturnsNewPair() {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client);

        var resp = await client.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = login.RefreshToken });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await resp.Content.ReadFromJsonAsync<Result<RefreshDto>>())!.Data;
        body!.Token.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBe(login.RefreshToken, "rotation must replace the token");
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_Returns401() {
        var resp = await _factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = "not-a-real-token" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ReusedRotatedToken_RevokesEntireFamily() {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client);

        // First rotation succeeds.
        var first = await client.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = login.RefreshToken });
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstBody = (await first.Content.ReadFromJsonAsync<Result<RefreshDto>>())!.Data!;

        // Replaying the original (now-rotated) refresh token must fail and
        // also revoke the freshly-issued one in the same family.
        var replay = await client.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = login.RefreshToken });
        replay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var afterReplay = await client.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = firstBody.RefreshToken });
        afterReplay.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "reuse detection must revoke the entire token family");
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken() {
        var client = _factory.CreateClient();
        var login = await LoginAsync(client);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login.Token);

        var logout = await client.PostAsJsonAsync("/api/auth/logout",
            new { RefreshToken = login.RefreshToken });
        logout.IsSuccessStatusCode.Should().BeTrue();

        var resp = await _factory.CreateClient().PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = login.RefreshToken });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record RefreshDto(string Token, string RefreshToken);
}
