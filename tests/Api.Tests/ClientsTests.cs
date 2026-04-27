using System.Net;
using System.Net.Http.Json;
using Contracts.Clients;
using Contracts.Results;
using FluentAssertions;

namespace Api.Tests;

[Collection(IntegrationCollection.Name)]
public class ClientsTests {
    private readonly HttpClient _client;

    public ClientsTests(OpenPsaFactory factory) {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task CreateClient_WithName_ReturnsCreated() {
        var request = new CreateClientRequest {
            Name = $"Acme {Guid.NewGuid():N}",
            Website = "https://acme.test",
            Email = "ops@acme.test",
            Status = ClientStatus.Active
        };

        var response = await _client.PostAsJsonAsync("/api/clients", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Result<ClientDto>>();
        result!.Data!.Name.Should().Be(request.Name);
        result.Data.Status.Should().Be(ClientStatus.Active);
        result.Data.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetClientById_AfterCreate_ReturnsClient() {
        var created = await CreateAsync();

        var response = await _client.GetAsync($"/api/clients/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<ClientDto>>();
        result!.Data!.Id.Should().Be(created.Id);
        result.Data.Name.Should().Be(created.Name);
    }

    [Fact]
    public async Task GetAllClients_ReturnsCreatedClient() {
        var created = await CreateAsync();

        var response = await _client.GetAsync("/api/clients?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ClientSummaryDto>>();
        result!.Data.Should().Contain(c => c.Id == created.Id);
    }

    [Fact]
    public async Task UpdateClient_ChangesNameAndStatus() {
        var created = await CreateAsync();

        var update = new UpdateClientRequest {
            Name = created.Name + " (renamed)",
            Status = ClientStatus.Inactive
        };

        var response = await _client.PutAsJsonAsync($"/api/clients/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<ClientDto>>();
        result!.Data!.Name.Should().EndWith("(renamed)");
        result.Data.Status.Should().Be(ClientStatus.Inactive);
    }

    [Fact]
    public async Task DeleteClient_ReturnsOk_AndGetReturns404() {
        var created = await CreateAsync();

        var deleteResponse = await _client.DeleteAsync($"/api/clients/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/clients/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<ClientDto> CreateAsync() {
        var response = await _client.PostAsJsonAsync("/api/clients", new CreateClientRequest {
            Name = $"Test Client {Guid.NewGuid():N}",
            Status = ClientStatus.Active
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Result<ClientDto>>();
        return result!.Data!;
    }
}
