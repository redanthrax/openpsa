using System.Net;
using System.Net.Http.Json;
using Contracts.Clients;
using Contracts.Results;
using Contracts.Tickets;
using FluentAssertions;

namespace Api.Tests;

/// <summary>
/// Exercises the create-ticket cross-module path: Tickets -> Wolverine -> Clients/Projects/Auth.
/// If module boundaries break (missing handler, contract drift), this test catches it.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class TicketsTests {
    private readonly HttpClient _client;

    public TicketsTests(OpenPsaFactory factory) {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task CreateTicket_WithValidClient_ReturnsCreatedAndResolvesClientName() {
        var (clientId, clientName) = await CreateClientAsync();

        var request = new CreateTicketRequest {
            Title = "Printer is on fire",
            Description = "Smoke detected at 09:14",
            Priority = TicketPriority.High,
            Type = TicketType.Incident,
            ClientId = clientId
        };

        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Result<TicketDto>>();
        result!.Data!.Title.Should().Be(request.Title);
        result.Data.Priority.Should().Be(TicketPriority.High);
        result.Data.ClientId.Should().Be(clientId);
        result.Data.ClientName.Should().Be(clientName);
        result.Data.Status.Should().Be(TicketStatus.New);
    }

    [Fact]
    public async Task CreateTicket_WithUnknownClient_Returns404() {
        var request = new CreateTicketRequest {
            Title = "Orphan ticket",
            ClientId = Guid.NewGuid(),
            Priority = TicketPriority.Low,
            Type = TicketType.Incident
        };

        var response = await _client.PostAsJsonAsync("/api/tickets", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListTickets_AfterCreate_IncludesNewTicket() {
        var (clientId, _) = await CreateClientAsync();

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest {
            Title = $"Ticket {Guid.NewGuid():N}",
            ClientId = clientId,
            Priority = TicketPriority.Medium,
            Type = TicketType.ServiceRequest
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createResponse.Content.ReadFromJsonAsync<Result<TicketDto>>())!.Data!;

        var listResponse = await _client.GetAsync("/api/tickets?page=1&pageSize=100");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Tickets list returns either Result<List<...>> or PagedResult<...> depending on endpoint.
        // Read raw and assert the id appears.
        var body = await listResponse.Content.ReadAsStringAsync();
        body.Should().Contain(created.Id.ToString());
    }

    private async Task<(Guid Id, string Name)> CreateClientAsync() {
        var name = $"Tickets Test Client {Guid.NewGuid():N}";
        var response = await _client.PostAsJsonAsync("/api/clients",
            new CreateClientRequest { Name = name, Status = ClientStatus.Active });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Result<ClientDto>>();
        return (result!.Data!.Id, name);
    }
}
