using System.Net;
using System.Net.Http.Json;
using Contracts.Results;
using Contracts.Sla;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests;

[Collection(IntegrationCollection.Name)]
public class SlaTests {
    private readonly HttpClient _fullClient;
    private readonly HttpClient _limitedClient;
    private readonly OpenPsaFactory _factory;

    public SlaTests(OpenPsaFactory factory) {
        _factory = factory;
        _fullClient = factory.CreateAuthenticatedClient();
        _limitedClient = factory.CreateClientWithoutSla();
    }

    [Fact]
    public async Task ListPolicies_WithPermission_Returns200AndSeededPolicies() {
        // TODO: If SlaPolicySummaryDto not found, adjust to read raw JSON
        var response = await _fullClient.GetAsync("/api/sla-policies?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SlaPolicySummaryDto>>();
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2); // Seeded standard and premium
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ListPolicies_WithoutPermission_Returns403() {
        var response = await _limitedClient.GetAsync("/api/sla-policies");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetSlaPolicyById_WithPermission_ReturnsPolicy() {
        var listResponse = await _fullClient.GetAsync("/api/sla-policies?page=1&pageSize=100");
        var listResult = await listResponse.Content.ReadFromJsonAsync<PagedResult<SlaPolicySummaryDto>>();
        var expected = listResult!.Data.First();

        var response = await _fullClient.GetAsync($"/api/sla-policies/{expected.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<SlaPolicyDto>>();
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(expected.Name);
    }

    [Fact]
    public async Task ListPolicies_WithNoPageParams_ReturnsAll() {
        var response = await _fullClient.GetAsync("/api/sla-policies");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<SlaPolicySummaryDto>>();
        result.Data.Should().HaveCount(2);
    }
}
