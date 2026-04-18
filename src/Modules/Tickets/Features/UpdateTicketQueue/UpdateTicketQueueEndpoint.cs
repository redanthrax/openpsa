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

namespace OpenPsa.Modules.Tickets.Features.UpdateTicketQueue;

public class UpdateTicketQueueEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/ticket-queues/{id:guid}", async (Guid id, UpdateTicketQueueRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var queue = await db.Set<TicketQueue>().FirstOrDefaultAsync(q => q.Id == id, ct);
            if (queue == null) return Results.NotFound();

            queue.Name = request.Name;
            queue.Description = request.Description;
            queue.AssignmentStrategy = request.AssignmentStrategy;
            queue.DefaultSlaPolicyId = request.DefaultSlaPolicyId;
            queue.SortOrder = request.SortOrder;
            queue.IsActive = request.IsActive;

            await db.SaveChangesAsync(ct);

            var openCount = await db.Set<Ticket>()
                .CountAsync(t => t.QueueId == queue.Id && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved, ct);

            return Results.Ok(Result.Ok(new TicketQueueDto {
                Id = queue.Id,
                Name = queue.Name,
                Description = queue.Description,
                AssignmentStrategy = queue.AssignmentStrategy,
                DefaultSlaPolicyId = queue.DefaultSlaPolicyId,
                SortOrder = queue.SortOrder,
                IsActive = queue.IsActive,
                OpenTicketCount = openCount,
                CreatedAt = queue.CreatedAt,
                UpdatedAt = queue.UpdatedAt
            }));
        }).RequirePermission("ticket-queues.update").WithTags("Ticket Queues");
    }
}
