using System.Net;
using System.Net.Http.Json;
using Contracts.Clients;
using Contracts.Results;
using Contracts.TimeEntries;
using FluentAssertions;

namespace Api.Tests;

[Collection(IntegrationCollection.Name)]
public class TimeEntriesTests {
    private readonly HttpClient _client;

    public TimeEntriesTests(OpenPsaFactory factory) {
        _client = factory.CreateAuthenticatedClient();
    }

    private async Task<Guid> CreateClientAsync() {
        var request = new CreateClientRequest { Name = $"Test Client {Guid.NewGuid():N}" };
        var response = await _client.PostAsJsonAsync("/api/clients", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Result<ClientDto>>();
        return result!.Data!.Id;
    }

    [Fact]
    public async Task CreateTimeEntry_WithBillableAndTicket_ReturnsCreated() {
        var clientId = await CreateClientAsync();

        var entry = new CreateTimeEntryRequest {
            ClientId = clientId,
            Date = DateTime.UtcNow.Date,
            Hours = 2.5m,
            Description = "Integration test work",
            Billable = true
        };

        var response = await _client.PostAsJsonAsync("/api/time-entries", entry);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<Result<TimeEntryDto>>();
        result!.Data!.Billable.Should().BeTrue();
        result.Data.Hours.Should().Be(2.5m);
        result.Data.ClientId.Should().Be(clientId);
    }

    [Fact]
    public async Task CreateTimeEntry_NonBillable_ReturnsBillableFalse() {
        var clientId = await CreateClientAsync();

        var entry = new CreateTimeEntryRequest {
            ClientId = clientId,
            Date = DateTime.UtcNow.Date,
            Hours = 1m,
            Billable = false
        };

        var response = await _client.PostAsJsonAsync("/api/time-entries", entry);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<Result<TimeEntryDto>>();
        result!.Data!.Billable.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllTimeEntries_ReturnsEntries() {
        var clientId = await CreateClientAsync();

        await _client.PostAsJsonAsync("/api/time-entries", new CreateTimeEntryRequest {
            ClientId = clientId, Date = DateTime.UtcNow.Date, Hours = 1m, Billable = true
        });

        var response = await _client.GetAsync("/api/time-entries");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<List<TimeEntryDto>>>();
        result!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateTimeEntry_ChangesBillableFlag() {
        var clientId = await CreateClientAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/time-entries", new CreateTimeEntryRequest {
            ClientId = clientId, Date = DateTime.UtcNow.Date, Hours = 1m, Billable = true
        });
        var created = (await createResponse.Content.ReadFromJsonAsync<Result<TimeEntryDto>>())!.Data!;

        var update = new UpdateTimeEntryRequest {
            Date = DateTime.UtcNow.Date,
            Hours = 1.5m,
            Billable = false
        };

        var response = await _client.PutAsJsonAsync($"/api/time-entries/{created.Id}", update);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Result<TimeEntryDto>>();
        result!.Data!.Billable.Should().BeFalse();
        result.Data.Hours.Should().Be(1.5m);
    }

    [Fact]
    public async Task DeleteTimeEntry_ReturnsOk() {
        var clientId = await CreateClientAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/time-entries", new CreateTimeEntryRequest {
            ClientId = clientId, Date = DateTime.UtcNow.Date, Hours = 1m, Billable = true
        });
        var created = (await createResponse.Content.ReadFromJsonAsync<Result<TimeEntryDto>>())!.Data!;

        var response = await _client.DeleteAsync($"/api/time-entries/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
