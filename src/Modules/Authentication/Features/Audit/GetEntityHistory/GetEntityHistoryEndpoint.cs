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

namespace OpenPsa.Modules.Authentication.Features.Audit.GetEntityHistory;

public class GetEntityHistoryEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/audit/{entityName}/{entityId}", async (
            string entityName,
            string entityId,
            int page = 1, int pageSize = 25,
            OpenPsaDbContext db = default!,
            CancellationToken ct = default) => {

                var query = db.Set<AuditEntry>()
                    .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                    .OrderByDescending(a => a.CreatedAt);

                var totalCount = await query.CountAsync(ct);
                var entries = await query
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

                return Results.Ok(PagedResult.Ok(entries, totalCount, page, pageSize));
            }).RequirePermission("audit.entity").WithTags("Audit");
    }
}
