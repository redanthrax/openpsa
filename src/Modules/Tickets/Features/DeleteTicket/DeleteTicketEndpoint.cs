using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Tickets.Models;

namespace OpenPsa.Modules.Tickets.Features.DeleteTicket;

public class DeleteTicketEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/tickets/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var ticket = await db.Set<Ticket>().FirstOrDefaultAsync(t => t.Id == id, ct);
            if (ticket == null) return Results.NotFound();

            db.Set<Ticket>().Remove(ticket);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("tickets.delete").WithTags("Tickets");
    }
}
