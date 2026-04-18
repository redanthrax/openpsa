using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Tickets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Features.GetAllTicketQueues;

public class GetAllTicketQueuesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/ticket-queues", async (OpenPsaDbContext db, CancellationToken ct) => {
            var queues = await db.Set<TicketQueue>()
                .OrderBy(q => q.SortOrder)
                .ThenBy(q => q.Name)
                .ToListAsync(ct);

            var queueIds = queues.Select(q => q.Id).ToList();
            var openCounts = await db.Set<Ticket>()
                .Where(t => t.QueueId.HasValue && queueIds.Contains(t.QueueId.Value)
                    && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved)
                .GroupBy(t => t.QueueId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);

            var dtos = queues.Select(q => new TicketQueueDto {
                Id = q.Id,
                Name = q.Name,
                Description = q.Description,
                AssignmentStrategy = q.AssignmentStrategy,
                DefaultSlaPolicyId = q.DefaultSlaPolicyId,
                SortOrder = q.SortOrder,
                IsActive = q.IsActive,
                OpenTicketCount = openCounts.GetValueOrDefault(q.Id, 0),
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            });

            return Results.Ok(Result.Ok(dtos));
        }).RequirePermission("ticket-queues.list").WithTags("Ticket Queues");
    }
}
