using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Sla;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Features.CreateSlaPolicy;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.UpdateSlaPolicy;

public class UpdateSlaPolicyEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPut("/api/sla-policies/{id:guid}", async (Guid id, UpdateSlaPolicyRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            var policy = await db.Set<SlaPolicy>().Include(p => p.Targets).FirstOrDefaultAsync(p => p.Id == id, ct);
            if (policy == null) return Results.NotFound();

            if (request.IsDefault && !policy.IsDefault) {
                var existing = await db.Set<SlaPolicy>().Where(p => p.IsDefault && p.Id != id).ToListAsync(ct);
                foreach (var p in existing) p.IsDefault = false;
            }

            policy.Name = request.Name;
            policy.Description = request.Description;
            policy.IsDefault = request.IsDefault;
            policy.BusinessHoursCalendarId = request.BusinessHoursCalendarId;

            db.Set<SlaTarget>().RemoveRange(policy.Targets);
            policy.Targets = request.Targets.Select(t => new SlaTarget {
                Priority = t.Priority,
                ResponseTimeMinutes = t.ResponseTimeMinutes,
                ResolutionTimeMinutes = t.ResolutionTimeMinutes
            }).ToList();

            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(CreateSlaPolicyEndpoint.MapToDto(policy)));
        }).RequirePermission("sla-policies.update").WithTags("SLA");
    }
}
