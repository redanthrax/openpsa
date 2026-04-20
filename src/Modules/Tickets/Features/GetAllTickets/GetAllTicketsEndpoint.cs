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

namespace OpenPsa.Modules.Tickets.Features.GetAllTickets;

public class GetAllTicketsEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/tickets", async (
            OpenPsaDbContext db, IMessageBus bus,
            Guid? clientId, Guid? projectId, TicketStatus? status,
            int page = 1, int pageSize = 25,
            CancellationToken ct = default) => {

            var query = db.Set<Ticket>().AsQueryable();
            if (clientId.HasValue) query = query.Where(t => t.ClientId == clientId.Value);
            if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId.Value);
            if (status.HasValue) query = query.Where(t => t.Status == status.Value);

            var ordered = query.OrderByDescending(t => t.CreatedAt);
            var totalCount = await ordered.CountAsync(ct);
            var tickets = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var clientIds = tickets.Select(t => t.ClientId).Distinct().ToList();
            var clientNames = (await bus.InvokeAsync<GetClientNamesResponse>(new GetClientNamesQuery(clientIds), ct)).Names;

            var projIds = tickets.Where(t => t.ProjectId.HasValue).Select(t => t.ProjectId!.Value).Distinct().ToList();
            var projectNames = (await bus.InvokeAsync<GetProjectNamesResponse>(new GetProjectNamesQuery(projIds), ct)).Names;

            var assigneeIds = tickets.Where(t => t.AssignedToUserId != null)
                .Select(t => Guid.Parse(t.AssignedToUserId!)).Distinct().ToList();
            var userNames = (await bus.InvokeAsync<GetUserNamesResponse>(new GetUserNamesQuery(assigneeIds), ct)).Names;

            var dtos = tickets.Select(t => new TicketDto {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                Type = t.Type,
                ClientId = t.ClientId,
                ClientName = clientNames.GetValueOrDefault(t.ClientId, string.Empty),
                ProjectId = t.ProjectId,
                ProjectName = t.ProjectId.HasValue ? projectNames.GetValueOrDefault(t.ProjectId.Value) : null,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUserId != null && Guid.TryParse(t.AssignedToUserId, out var uid)
                    ? userNames.GetValueOrDefault(uid)
                    : null,
                DueDate = t.DueDate,
                ResolvedAt = t.ResolvedAt,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();

            return Results.Ok(PagedResult.Ok<TicketDto>(dtos, totalCount, page, pageSize));
        }).RequirePermission("tickets.list").WithTags("Tickets");
    }
}
