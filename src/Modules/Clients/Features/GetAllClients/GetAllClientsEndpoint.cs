using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Clients;
using Contracts.Results;
using IntegrationEvents.Projects;
using IntegrationEvents.Tickets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Clients.Models;
using Wolverine;

namespace OpenPsa.Modules.Clients.Features.GetAllClients;

public class GetAllClientsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/clients", async (
            OpenPsaDbContext db, IMessageBus bus,
            int page = 1,
            int pageSize = 25,
            CancellationToken ct = default) => {

            var query = db.Set<Client>().OrderBy(c => c.Name);
            var totalCount = await query.CountAsync(ct);
            var clients = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var clientIds = clients.Select(c => c.Id).ToList();

            var projectCounts = clientIds.Count > 0
                ? (await bus.InvokeAsync<GetActiveProjectCountsByClientResponse>(
                    new GetActiveProjectCountsByClientQuery(clientIds), ct)).Counts
                : new Dictionary<Guid, int>();

            var ticketCounts = clientIds.Count > 0
                ? (await bus.InvokeAsync<GetOpenTicketCountsByClientResponse>(
                    new GetOpenTicketCountsByClientQuery(clientIds), ct)).Counts
                : new Dictionary<Guid, int>();

            var dtos = clients.Select(c => new ClientSummaryDto {
                Id = c.Id,
                Name = c.Name,
                Status = c.Status,
                ActiveProjects = projectCounts.GetValueOrDefault(c.Id, 0),
                OpenTickets = ticketCounts.GetValueOrDefault(c.Id, 0)
            }).ToList();

            return Results.Ok(PagedResult.Ok<ClientSummaryDto>(dtos, totalCount, page, pageSize));
        }).RequirePermission("clients.list").WithTags("Clients");
    }
}
