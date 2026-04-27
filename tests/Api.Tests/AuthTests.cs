using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Contracts.Results;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using OpenPsa.Modules.Authentication.Features.Auth.Login;

namespace Api.Tests;

[Collection(IntegrationCollection.Name)]
public class AuthTests {
    private readonly OpenPsaFactory _factory;

    public AuthTests(OpenPsaFactory factory) {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidAdminCreds_Returns200AndToken() {
        var client = _factory.CreateClient();
        var request = new LoginRequest("admin@openpsa.dev", "admin");

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<LoginResponse>>();
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.UserId.Should().NotBeNullOrEmpty();
        result.Data.Email.Should().Be("admin@openpsa.dev");
        result.Data.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401() {
        var client = _factory.CreateClient();
        var request = new LoginRequest("admin@openpsa.dev", "wrong");

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithMissingUser_Returns401() {
        var client = _factory.CreateClient();
        var request = new LoginRequest("nonexistent@openpsa.dev", "admin");

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithValidToken_Returns200() {
        // First login to get token
        var unauthClient = _factory.CreateClient();
        var loginRequest = new LoginRequest("admin@openpsa.dev", "admin");
        var loginResponse = await unauthClient.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<LoginResponse>>();
        var token = loginResult.Data.Token;

        var authClient = _factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var clientsResponse = await authClient.GetAsync("/api/clients");

        clientsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithExpiredToken_Returns401() {
        var expiredToken = _factory.GenerateExpiredToken(Guid.NewGuid(), "Admin");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await client.GetAsync("/api/clients");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithInvalidToken_Returns401() {
        var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.invalid";

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invalidToken);

        var response = await client.GetAsync("/api/clients");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
