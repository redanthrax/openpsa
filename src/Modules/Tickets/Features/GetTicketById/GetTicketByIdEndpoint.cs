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

namespace OpenPsa.Modules.Tickets.Features.GetTicketById;

public class GetTicketByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/tickets/{id:guid}", async (Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var ticket = await db.Set<Ticket>().FirstOrDefaultAsync(t => t.Id == id, ct);
            if (ticket == null) return Results.NotFound();

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(ticket.ClientId), ct)).Name ?? string.Empty;

            string? projectName = null;
            if (ticket.ProjectId.HasValue)
                projectName = (await bus.InvokeAsync<GetProjectNameResponse>(new GetProjectNameQuery(ticket.ProjectId.Value), ct)).Name;

            string? assigneeName = null;
            if (ticket.AssignedToUserId != null && Guid.TryParse(ticket.AssignedToUserId, out var uid))
                assigneeName = (await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(uid), ct)).Name;

            return Results.Ok(Result.Ok(new TicketDto {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                Type = ticket.Type,
                ClientId = ticket.ClientId,
                ClientName = clientName,
                ProjectId = ticket.ProjectId,
                ProjectName = projectName,
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedToUserName = assigneeName,
                DueDate = ticket.DueDate,
                ResolvedAt = ticket.ResolvedAt,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            }));
        }).RequirePermission("tickets.view").WithTags("Tickets");
    }
}
