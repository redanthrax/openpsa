using Common.Audit;
using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Audit;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace OpenPsa.Modules.Authentication.Features.Audit.GetAuditEntries;

public class GetAuditEntriesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/audit", async (
            OpenPsaDbContext db,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default) => {

            var query = db.Set<AuditEntry>().OrderByDescending(a => a.CreatedAt);
            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditEntryDto {
                    Id = a.Id,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Action = a.Action.ToString(),
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    ChangedProperties = a.ChangedProperties,
                    UserId = a.UserId,
                    UserEmail = a.UserEmail,
                    UserName = a.UserName,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync(ct);

            return Results.Ok(PagedResult.Ok(items, total, page, pageSize));
        }).RequirePermission("audit.list").WithTags("Audit");
    }
}
