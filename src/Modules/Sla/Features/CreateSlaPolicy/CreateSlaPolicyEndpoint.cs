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

namespace OpenPsa.Modules.Sla.Features.CreateSlaPolicy;

public class CreateSlaPolicyEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapPost("/api/sla-policies", async (CreateSlaPolicyRequest request, OpenPsaDbContext db, CancellationToken ct) => {
            if (request.IsDefault) {
                var existing = await db.Set<SlaPolicy>().Where(p => p.IsDefault).ToListAsync(ct);
                foreach (var p in existing) p.IsDefault = false;
            }

            var policy = new SlaPolicy {
                Name = request.Name,
                Description = request.Description,
                IsDefault = request.IsDefault,
                Targets = request.Targets.Select(t => new SlaTarget {
                    Priority = t.Priority,
                    ResponseTimeMinutes = t.ResponseTimeMinutes,
                    ResolutionTimeMinutes = t.ResolutionTimeMinutes
                }).ToList()
            };

            db.Set<SlaPolicy>().Add(policy);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/sla-policies/{policy.Id}", Result.Ok(MapToDto(policy)));
        }).RequirePermission("sla-policies.create").WithTags("SLA");
    }

    internal static SlaPolicyDto MapToDto(SlaPolicy policy) => new() {
        Id = policy.Id,
        Name = policy.Name,
        Description = policy.Description,
        IsDefault = policy.IsDefault,
        Targets = policy.Targets.Select(t => new SlaTargetDto {
            Id = t.Id,
            Priority = t.Priority,
            ResponseTimeMinutes = t.ResponseTimeMinutes,
            ResolutionTimeMinutes = t.ResolutionTimeMinutes
        }).ToList(),
        CreatedAt = policy.CreatedAt,
        UpdatedAt = policy.UpdatedAt
    };
}
