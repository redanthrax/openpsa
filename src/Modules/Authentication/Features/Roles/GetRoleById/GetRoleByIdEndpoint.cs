using Common.Authorization;
using Common.Database;
using Common.Modules;
using Contracts.Results;
using Contracts.Roles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Authentication.Models;

namespace OpenPsa.Modules.Authentication.Features.Roles.GetRoleById;

public class GetRoleByIdEndpoint : IEndpointFeature {
    public static void MapEndpoint(IEndpointRouteBuilder app) {
        app.MapGet("/api/roles/{id:guid}", async (Guid id, OpenPsaDbContext db, CancellationToken ct) => {
            var role = await db.Set<Role>().FirstOrDefaultAsync(r => r.Id == id, ct);
            if (role == null) return Results.NotFound();

            return Results.Ok(Result.Ok(new RoleDto {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Permissions = role.PermissionKeys
            }));
        }).RequirePermission("roles.view").WithTags("Roles");
    }
}
