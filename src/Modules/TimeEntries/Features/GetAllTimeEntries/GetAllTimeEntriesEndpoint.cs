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

namespace OpenPsa.Modules.TimeEntries.Features.GetAllTimeEntries;

public class GetAllTimeEntriesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/time-entries", async (OpenPsaDbContext db, IMessageBus bus, IUserContext userContext, Guid? clientId, Guid? projectId, bool? myEntries, CancellationToken ct) => {
            var query = db.Set<TimeEntry>().AsQueryable();
            if (clientId.HasValue) query = query.Where(t => t.ClientId == clientId.Value);
            if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId.Value);
            if (myEntries == true && Guid.TryParse(userContext.UserId, out var meId))
                query = query.Where(t => t.UserId == meId);

            var entries = await query.OrderByDescending(t => t.Date).ToListAsync(ct);

            var clientIds = entries.Select(t => t.ClientId).Distinct().ToList();
            var clientNames = (await bus.InvokeAsync<GetClientNamesResponse>(new GetClientNamesQuery(clientIds), ct)).Names;

            var projIds = entries.Where(t => t.ProjectId.HasValue).Select(t => t.ProjectId!.Value).Distinct().ToList();
            var projectNames = (await bus.InvokeAsync<GetProjectNamesResponse>(new GetProjectNamesQuery(projIds), ct)).Names;

            var ticketIds = entries.Where(t => t.TicketId.HasValue).Select(t => t.TicketId!.Value).Distinct().ToList();
            var ticketTitles = (await bus.InvokeAsync<GetTicketTitlesResponse>(new GetTicketTitlesQuery(ticketIds), ct)).Titles;

            var userIds = entries.Select(t => t.UserId).Distinct().ToList();
            var userNames = (await bus.InvokeAsync<GetUserNamesResponse>(new GetUserNamesQuery(userIds), ct)).Names;

            var dtos = entries.Select(t => new TimeEntryDto {
                Id = t.Id,
                ClientId = t.ClientId,
                ClientName = clientNames.GetValueOrDefault(t.ClientId, string.Empty),
                ProjectId = t.ProjectId,
                ProjectName = t.ProjectId.HasValue ? projectNames.GetValueOrDefault(t.ProjectId.Value) : null,
                TicketId = t.TicketId,
                TicketTitle = t.TicketId.HasValue ? ticketTitles.GetValueOrDefault(t.TicketId.Value) : null,
                UserId = t.UserId.ToString(),
                UserName = userNames.GetValueOrDefault(t.UserId, string.Empty),
                Date = t.Date,
                Hours = t.Hours,
                Description = t.Description,
                Billable = t.Billable,
                Invoiced = t.Invoiced,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });

            return Results.Ok(Result.Ok(dtos));
        }).RequirePermission("time-entries.list").WithTags("Time Entries");
    }
}
