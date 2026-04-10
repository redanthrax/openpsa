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

namespace OpenPsa.Modules.TimeEntries.Features.UpdateTimeEntry;

public class UpdateTimeEntryEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/time-entries/{id:guid}", async (Guid id, UpdateTimeEntryRequest request, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var entry = await db.Set<TimeEntry>().FirstOrDefaultAsync(t => t.Id == id, ct);
            if (entry == null) return Results.NotFound();

            var oldHours = entry.Hours;
            var oldProjectId = entry.ProjectId;

            entry.ProjectId = request.ProjectId;
            entry.TicketId = request.TicketId;
            entry.Date = request.Date;
            entry.Hours = request.Hours;
            entry.Description = request.Description;
            entry.Billable = request.Billable;

            await db.SaveChangesAsync(ct);

            await bus.PublishAsync(new IntegrationEvents.TimeEntries.TimeEntryUpdated(entry.Id, entry.ProjectId, entry.Hours, entry.Billable, entry.Invoiced));

            var clientName = (await bus.InvokeAsync<GetClientNameResponse>(new GetClientNameQuery(entry.ClientId), ct)).Name ?? string.Empty;
            var userName = (await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(entry.UserId), ct)).Name ?? string.Empty;
            string? projectName = entry.ProjectId.HasValue ? (await bus.InvokeAsync<GetProjectNameResponse>(new GetProjectNameQuery(entry.ProjectId.Value), ct)).Name : null;
            string? ticketTitle = entry.TicketId.HasValue ? (await bus.InvokeAsync<GetTicketTitleResponse>(new GetTicketTitleQuery(entry.TicketId.Value), ct)).Title : null;

            return Results.Ok(Result.Ok(new TimeEntryDto {
                Id = entry.Id,
                ClientId = entry.ClientId,
                ClientName = clientName,
                ProjectId = entry.ProjectId,
                ProjectName = projectName,
                TicketId = entry.TicketId,
                TicketTitle = ticketTitle,
                UserId = entry.UserId.ToString(),
                UserName = userName,
                Date = entry.Date,
                Hours = entry.Hours,
                Description = entry.Description,
                Billable = entry.Billable,
                Invoiced = entry.Invoiced,
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt
            }));
        }).RequirePermission("time-entries.update").WithTags("Time Entries");
    }
}
