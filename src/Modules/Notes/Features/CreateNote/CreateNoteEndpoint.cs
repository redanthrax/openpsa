using Common.Authentication;
using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Notes;
using Contracts.Results;
using IntegrationEvents.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenPsa.Modules.Notes.Models;
using Wolverine;

namespace OpenPsa.Modules.Notes.Features.CreateNote;

public class CreateNoteEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/notes", async (CreateNoteRequest request, OpenPsaDbContext db, IMessageBus bus, IUserContext userContext, CancellationToken ct) => {
            if (!Guid.TryParse(userContext.UserId, out var userId))
                return Results.Unauthorized();

            var note = new Note {
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                Content = request.Content,
                UserId = userId
            };

            db.Set<Note>().Add(note);
            await db.SaveChangesAsync(ct);

            var userName = (await bus.InvokeAsync<GetUserNameResponse>(new GetUserNameQuery(userId), ct)).Name ?? string.Empty;

            return Results.Created($"/api/notes/{note.Id}", Result.Ok(new NoteDto {
                Id = note.Id,
                EntityType = note.EntityType,
                EntityId = note.EntityId,
                Content = note.Content,
                UserId = note.UserId.ToString(),
                UserName = userName,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            }));
        }).RequirePermission("notes.create").WithTags("Notes");
    }
}
