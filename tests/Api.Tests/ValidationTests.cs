using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OpenPsa.Modules.Authentication.Features.Auth.Login;

namespace Api.Tests;

[Collection(IntegrationCollection.Name)]
public class ValidationTests {
    private readonly OpenPsaFactory _factory;

    public ValidationTests(OpenPsaFactory factory) {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithEmptyEmail_Returns400ProblemDetails() {
        var client = _factory.CreateClient();
        var request = new LoginRequest("", "admin");

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        doc.RootElement.GetProperty("errors").GetProperty("Email").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WithMalformedEmail_Returns400() {
        var client = _factory.CreateClient();
        var request = new LoginRequest("not-an-email", "admin");

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
