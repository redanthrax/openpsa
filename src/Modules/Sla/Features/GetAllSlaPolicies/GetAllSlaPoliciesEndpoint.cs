using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Sla;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.GetAllSlaPolicies;

public class GetAllSlaPoliciesEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/sla-policies", async (OpenPsaDbContext db, CancellationToken ct) => {
            var policies = await db.Set<SlaPolicy>()
                .Include(p => p.Targets)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);

            var dtos = policies.Select(p => new SlaPolicySummaryDto {
                Id = p.Id,
                Name = p.Name,
                IsDefault = p.IsDefault,
                TargetCount = p.Targets.Count
            });

            return Results.Ok(Result.Ok(dtos));
        }).RequirePermission("sla-policies.list").WithTags("SLA");
    }
}
