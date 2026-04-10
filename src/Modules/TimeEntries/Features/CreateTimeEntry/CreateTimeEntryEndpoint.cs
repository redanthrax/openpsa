using Common.Authentication;
using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.TimeEntries;
using IntegrationEvents.Authentication;
using IntegrationEvents.Clients;
using IntegrationEvents.Projects;
using IntegrationEvents.Tickets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.TimeEntries.Models;
using Wolverine;

namespace OpenPsa.Modules.TimeEntries.Features.CreateTimeEntry;

public class CreateTimeEntryEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/time-entries", async (CreateTimeEntryRequest request, OpenPsaDbContext db, IMessageBus bus, IUserContext userContext, CancellationToken ct) => {
            if (!Guid.TryParse(userContext.UserId, out var userId))
                return Results.Unauthorized();

            var clientResponse = await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(request.ClientId), ct);
            if (!clientResponse.Found)
                return Results.Json(Result.Fail<TimeEntryDto>("Client not found"), statusCode: 404);

            string? projectName = null;
            if (request.ProjectId.HasValue)
                projectName = (await bus.InvokeAsync<GetProjectNameResponse>(new GetProjectNameQuery(request.ProjectId.Value), ct)).Name;

            string? ticketTitle = null;
            if (request.TicketId.HasValue)
                ticketTitle = (await bus.InvokeAsync<GetTicketTitleResponse>(new GetTicketTitleQuery(request.TicketId.Value), ct)).Title;

            var userResponse = await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(userId), ct);

            var entry = new TimeEntry {
                ClientId = request.ClientId,
                ProjectId = request.ProjectId,
                TicketId = request.TicketId,
                UserId = userId,
                Date = request.Date,
                Hours = request.Hours,
                Description = request.Description,
                Billable = request.Billable
            };

            db.Set<TimeEntry>().Add(entry);
            await db.SaveChangesAsync(ct);

            await bus.PublishAsync(new IntegrationEvents.TimeEntries.TimeEntryLogged(entry.Id, entry.ClientId, entry.ProjectId, entry.Hours, entry.Billable, entry.Invoiced));

            return Results.Created($"/api/time-entries/{entry.Id}", Result.Ok(new TimeEntryDto {
                Id = entry.Id,
                ClientId = entry.ClientId,
                ClientName = clientResponse.Name ?? string.Empty,
                ProjectId = entry.ProjectId,
                ProjectName = projectName,
                TicketId = entry.TicketId,
                TicketTitle = ticketTitle,
                UserId = entry.UserId.ToString(),
                UserName = userResponse.Name ?? string.Empty,
                Date = entry.Date,
                Hours = entry.Hours,
                Description = entry.Description,
                Billable = entry.Billable,
                Invoiced = entry.Invoiced,
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt
            }));
        }).RequirePermission("time-entries.create").WithTags("Time Entries");
    }
}
