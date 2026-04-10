using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.TimeEntries.Models;
using Wolverine;

namespace OpenPsa.Modules.TimeEntries.Features.DeleteTimeEntry;

public class DeleteTimeEntryEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/time-entries/{id:guid}", async (Guid id, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var entry = await db.Set<TimeEntry>().FirstOrDefaultAsync(t => t.Id == id, ct);
            if (entry == null) return Results.NotFound();

            db.Set<TimeEntry>().Remove(entry);
            await db.SaveChangesAsync(ct);

            await bus.PublishAsync(new IntegrationEvents.TimeEntries.TimeEntryDeleted(entry.Id, entry.ProjectId, entry.Hours));

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("time-entries.delete").WithTags("Time Entries");
    }
}
