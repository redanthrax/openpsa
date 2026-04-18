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

namespace OpenPsa.Modules.Sla.Features.GetSlaPolicyById;

public class GetSlaPolicyByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/sla-policies/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var policy = await db.Set<SlaPolicy>().Include(p => p.Targets).FirstOrDefaultAsync(p => p.Id == id, ct);
            if (policy == null) return Results.NotFound();

            return Results.Ok(Result.Ok(CreateSlaPolicyEndpoint.MapToDto(policy)));
        }).RequirePermission("sla-policies.view").WithTags("SLA");
    }
}
