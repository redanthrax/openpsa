using Common.Authorization;
using Common.Authentication;
using Common.Database;
using Common.Modules;
using Contracts.Notes;
using Contracts.Results;
using IntegrationEvents.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Notes.Models;
using Wolverine;

namespace OpenPsa.Modules.Notes.Features.GetNotes;

public class GetNotesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/notes/{entityType}/{entityId:guid}", async (string entityType, Guid entityId, OpenPsaDbContext db, IMessageBus bus, CancellationToken ct) => {
            var notes = await db.Set<Note>()
                .Where(n => n.EntityType == entityType && n.EntityId == entityId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(ct);

            var userIds = notes.Select(n => n.UserId).Distinct().ToList();
            var userNames = (await bus.InvokeAsync<GetUserNamesResponse>(new GetUserNamesQuery(userIds), ct)).Names;

            var dtos = notes.Select(n => new NoteDto {
                Id = n.Id,
                EntityType = n.EntityType,
                EntityId = n.EntityId,
                Content = n.Content,
                UserId = n.UserId.ToString(),
                UserName = userNames.GetValueOrDefault(n.UserId, string.Empty),
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            });

            return Results.Ok(Result.Ok(dtos));
        }).RequirePermission("notes.list").WithTags("Notes");
    }
}
