using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Features.Roles.DeleteRole;

public class DeleteRoleEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapDelete("/api/roles/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var role = await db.Set<Role>().FirstOrDefaultAsync(r => r.Id == id, ct);
            if (role == null) return Results.NotFound();

            db.Set<Role>().Remove(role);
            await db.SaveChangesAsync(ct);

            return Results.Ok(Result.Ok(true));
        }).RequirePermission("roles.delete").WithTags("Roles");
    }
}
