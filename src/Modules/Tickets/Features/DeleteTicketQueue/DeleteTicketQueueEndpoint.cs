using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Features.DeleteTicketQueue;

public class DeleteTicketQueueEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/ticket-queues/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var queue = await db.Set<TicketQueue>().FirstOrDefaultAsync(q => q.Id == id, ct);
            if (queue == null) return Results.NotFound();

            db.Set<TicketQueue>().Remove(queue);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("ticket-queues.delete").WithTags("Ticket Queues");
    }
}
