using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Tickets;
using IntegrationEvents.Authentication;
using IntegrationEvents.Clients;
using IntegrationEvents.Projects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Tickets.Models;
using Wolverine;

namespace OpenPsa.Modules.Tickets.Features.CreateTicket;

public class CreateTicketEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/tickets", async (CreateTicketRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(request.ClientId), ct);
            if (!clientResponse.Found)
                return Results.Json(Result.Fail<TicketDto>("Client not found"), statusCode: 404);

            string? projectName = null;
            if (request.ProjectId.HasValue)
                projectName = (await bus.InvokeAsync<GetProjectNameResponse>(new GetProjectNameQuery(request.ProjectId.Value), ct)).Name;

            string? assigneeName = null;
            if (request.AssignedToUserId != null && Guid.TryParse(request.AssignedToUserId, out var uid))
                assigneeName = (await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(uid), ct)).Name;

            var ticket = new Ticket {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                Type = request.Type,
                ClientId = request.ClientId,
                ProjectId = request.ProjectId,
                AssignedToUserId = request.AssignedToUserId,
                DueDate = request.DueDate
            };

            db.Set<Ticket>().Add(ticket);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/tickets/{ticket.Id}", Result.Ok(new TicketDto {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                Type = ticket.Type,
                ClientId = ticket.ClientId,
                ClientName = clientResponse.Name ?? string.Empty,
                ProjectId = ticket.ProjectId,
                ProjectName = projectName,
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedToUserName = assigneeName,
                DueDate = ticket.DueDate,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            }));
        }).RequirePermission("tickets.create").WithTags("Tickets");
    }
}
