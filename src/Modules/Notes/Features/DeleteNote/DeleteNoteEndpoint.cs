using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Notes.Models;

namespace OpenPsa.Modules.Notes.Features.DeleteNote;

public class DeleteNoteEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/notes/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var note = await db.Set<Note>().FirstOrDefaultAsync(n => n.Id == id, ct);
            if (note == null) return Results.NotFound();

            db.Set<Note>().Remove(note);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("notes.delete").WithTags("Notes");
    }
}
