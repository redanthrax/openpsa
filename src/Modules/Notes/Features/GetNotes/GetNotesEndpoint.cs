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
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Notes.Models;
using Wolverine;

namespace OpenPsa.Modules.Notes.Features.GetNotes;

public class GetNotesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/notes/{entityType}/{entityId:guid}", async (
            string entityType, Guid entityId,
            int page = 1, int pageSize = 25,
            OpenPsaDbContext db = default!, IMessageBus bus = default!,
            CancellationToken ct = default) => {

                var query = db.Set<Note>()
                    .Where(n => n.EntityType == entityType && n.EntityId == entityId)
                    .OrderByDescending(n => n.CreatedAt);

                var totalCount = await query.CountAsync(ct);
                var notes = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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
                    IsInternal = n.IsInternal,
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                }).ToList();

                return Results.Ok(PagedResult.Ok<NoteDto>(dtos, totalCount, page, pageSize));
            }).RequirePermission("notes.list").WithTags("Notes");
    }
}
