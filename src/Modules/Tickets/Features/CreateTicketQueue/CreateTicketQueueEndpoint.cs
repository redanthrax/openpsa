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

namespace OpenPsa.Modules.Tickets.Features.CreateTicketQueue;

public class CreateTicketQueueEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/ticket-queues", async (CreateTicketQueueRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var queue = new TicketQueue {
                Name = request.Name,
                Description = request.Description,
                AssignmentStrategy = request.AssignmentStrategy,
                DefaultSlaPolicyId = request.DefaultSlaPolicyId,
                SortOrder = request.SortOrder
            };

            db.Set<TicketQueue>().Add(queue);
            await db.SaveChangesAsync(ct);

            var openCount = await db.Set<Ticket>()
                .CountAsync(t => t.QueueId == queue.Id && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved, ct);

            return Results.Created($"/api/ticket-queues/{queue.Id}", Result.Ok(new TicketQueueDto {
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
        }).RequirePermission("ticket-queues.create").WithTags("Ticket Queues");
    }
}
