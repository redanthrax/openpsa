using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Sla.Models;

namespace OpenPsa.Modules.Sla.Features.DeleteSlaPolicy;

public class DeleteSlaPolicyEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/sla-policies/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var policy = await db.Set<SlaPolicy>().Include(p => p.Targets).FirstOrDefaultAsync(p => p.Id == id, ct);
            if (policy == null) return Results.NotFound();

            db.Set<SlaPolicy>().Remove(policy);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("sla-policies.delete").WithTags("SLA");
    }
}
